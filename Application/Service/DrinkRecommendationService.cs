using Application.DTOs.Drink;
using Application.DTOs.ML;
using Application.IService;
using AutoMapper;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Data;

namespace Application.Service
{
    public class DrinkRecommendationService : IDrinkRecommendationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MLContext _mlContext;
        private readonly IMapper _mapper;
        private ITransformer _model;
        private readonly string MODEL_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drink_emotion_model.zip");

        public DrinkRecommendationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mlContext = new MLContext(seed: 0);
            _mapper = mapper;
        }

        private void LoadModel()
        {
            try
            {
                if (File.Exists(MODEL_PATH))
                {
                    _model = _mlContext.Model.Load(MODEL_PATH, out var modelSchema);
                }
            }
            catch (Exception)
            {
                TrainModel().Wait();
            }
        }

        public async Task TrainModel(string trainingFilePath = null)
        {
            try 
            {
                Console.WriteLine("Bắt đầu lấy dữ liệu training...");
                IDataView trainingData;
                
                if (!string.IsNullOrEmpty(trainingFilePath) && File.Exists(trainingFilePath))
                {
                    // Load dữ liệu từ file nếu có đường dẫn
                    trainingData = LoadTrainingDataFromFile(trainingFilePath);
                    Console.WriteLine("Đã load dữ liệu từ file thành công!");
                }
                else
                {
                    // Nếu không có file, sử dụng dữ liệu từ database
                    trainingData = await GetTrainingData();
                    Console.WriteLine("Đã load dữ liệu từ database thành công!");
                }
                
                Console.WriteLine("Đang xây dựng pipeline...");
                var pipeline = BuildTrainingPipeline();

                Console.WriteLine("Bắt đầu training model...");
                // Thêm timeout cho quá trình training
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // Timeout sau 5 phút
                
                var trainTask = Task.Run(() => 
                {
                    _model = pipeline.Fit(trainingData);
                    return _model;
                }, cts.Token);

                try 
                {
                    await trainTask;
                    Console.WriteLine("Training hoàn tất, đang lưu model...");
                    _mlContext.Model.Save(_model, trainingData.Schema, MODEL_PATH);
                    Console.WriteLine("Đã lưu model thành công!");
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException("Training model đã timeout sau 5 phút");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong quá trình training: {ex.Message}");
                throw;
            }
        }

        private IEstimator<ITransformer> BuildTrainingPipeline()
        {
            return _mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "Label",
                    inputColumnName: nameof(DrinkEmotionData.EmotionCategory))
                .Append(_mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "CommentFeatures",
                    inputColumnName: nameof(DrinkEmotionData.CommentEmotional)))
                .Append(_mlContext.Transforms.Conversion.ConvertType(
                    outputColumnName: "RatingFloat",
                    inputColumnName: nameof(DrinkEmotionData.Rating),
                    outputKind: DataKind.Single))
                .Append(_mlContext.Transforms.Conversion.ConvertType(
                    outputColumnName: "PriceFloat",
                    inputColumnName: nameof(DrinkEmotionData.Price),
                    outputKind: DataKind.Single))
                .Append(_mlContext.Transforms.Concatenate(
                    "Features",
                    "CommentFeatures",
                    "RatingFloat",
                    "PriceFloat"))
                // Sử dụng trainer đơn giản hơn cho tập dữ liệu nhỏ
                .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(
                    labelColumnName: "Label",
                    featureColumnName: "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                    outputColumnName: "PredictedLabel",
                    inputColumnName: "PredictedLabel"));
        }

        public async Task<List<DrinkResponse>> GetDrinkRecommendationsBaseOnFeedback(string emotion, int count = 5)
        {
            try 
            {
                // Dự đoán cảm xúc từ input
                var predictedEmotion = await PredictEmotion(emotion);

                // Lấy danh sách category name hợp lệ từ EmotionalDrinkCategory
                var validCategories = (await _unitOfWork.EmotionalDrinkCategoryRepository
                    .GetAsync(
                        filter: e => e.IsDeleted == false,
                        orderBy: null,
                        includeProperties: ""))
                    .Select(e => e.CategoryName.ToLower())
                    .ToList();

                // Map predicted emotion với category gần nhất
                var matchedCategory = validCategories
                    .OrderBy(c => LevenshteinDistance(c, predictedEmotion.ToLower()))
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(matchedCategory))
                {
                    return new List<DrinkResponse>();
                }

                // Lấy danh sách đồ uống phù hợp với cảm xúc
                var recommendedDrinks = (await _unitOfWork.DrinkRepository
                    .GetAsync(
                        filter: d => d.DrinkEmotionalCategories
                            .Any(ec => ec.EmotionalDrinkCategory.CategoryName.ToLower() == matchedCategory)
                            && d.Status == true,
                        orderBy: q => q.OrderByDescending(d => d.BookingDrinks.Count),
                        includeProperties: "DrinkEmotionalCategories,DrinkEmotionalCategories.EmotionalDrinkCategory",
                        pageIndex: 1,
                        pageSize: count))
                    .ToList();

                return _mapper.Map<List<DrinkResponse>>(recommendedDrinks);
            }
            catch (Exception ex)
            {
                // Log error
                throw;
            }
        }

        public async Task<List<DrinkResponse>> GetDrinkRecommendations(string emotion, Guid barId)
        {
            try
            {
                var prompt = new EmotionModel.ModelInput()
                {
                    Text = emotion,
                };
                var result = EmotionModel.Predict(prompt);

                if(!result.PredictedLabel.IsNullOrEmpty())
                {
                    var emotionLabelUpper = result.PredictedLabel.ToUpper();
                    var drinks = await _unitOfWork.DrinkRepository.GetAsync(
                        filter: x => x.BarId.Equals(barId) && x.DrinkEmotionalCategories.Any(d => d.EmotionalDrinkCategory.CategoryName.ToUpper().Equals(emotionLabelUpper)),
                        includeProperties: "DrinkEmotionalCategories.EmotionalDrinkCategory");
                    return _mapper.Map<List<DrinkResponse>>(drinks.ToList());
                }

                return new List<DrinkResponse>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        // Hàm tính độ tương đồng giữa 2 chuỗi
        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int j = 1; j <= s2.Length; j++)
                for (int i = 1; i <= s1.Length; i++)
                    if (s1[i - 1] == s2[j - 1])
                        d[i, j] = d[i - 1, j - 1];
                    else
                        d[i, j] = Math.Min(Math.Min(
                            d[i - 1, j] + 1,     // Deletion
                            d[i, j - 1] + 1),    // Insertion
                            d[i - 1, j - 1] + 1); // Substitution

            return d[s1.Length, s2.Length];
        }

        private async Task<string> PredictEmotion(string input)
        {
            if (_model == null)
            {
                throw new InvalidOperationException("Model chưa được khởi tạo");
            }

            var predictionEngine = _mlContext.Model
                .CreatePredictionEngine<DrinkEmotionData, DrinkEmotionPrediction>(_model);

            var prediction = predictionEngine.Predict(new DrinkEmotionData 
            { 
                CommentEmotional = input,
                Rating = 5,
                Price = 0
            });

            return $"{prediction.Emotion}";
        }

        private async Task<IDataView> GetTrainingData()
        {
            var feedbacks = await _unitOfWork.FeedbackRepository
                .GetAsync(
                    filter: f => !string.IsNullOrEmpty(f.CommentEmotionalForDrink),
                    includeProperties: "Booking.BookingDrinks.Drink.DrinkEmotionalCategories.EmotionalDrinkCategory");

            var trainingData = feedbacks.Select(f => new DrinkEmotionData
            {
                CommentEmotional = f.CommentEmotionalForDrink,
                Rating = f.Rating,
                DrinkName = f.Booking?.BookingDrinks?.FirstOrDefault()?.Drink.DrinkName,
                Description = f.Booking?.BookingDrinks?.FirstOrDefault()?.Drink.Description,
                Price = f.Booking.BookingDrinks.FirstOrDefault()?.Drink.Price ?? 0,
                EmotionCategory = f.Booking.BookingDrinks
                    .FirstOrDefault()?.Drink.DrinkEmotionalCategories
                    .FirstOrDefault()?.EmotionalDrinkCategory.CategoryName
            }).ToList();

            return _mlContext.Data.LoadFromEnumerable(trainingData);
        }

        private IDataView LoadTrainingDataFromFile(string filePath)
        {
            try
            {
                // Thêm validation trước khi load dữ liệu
                ValidateTrainingData(filePath);

                // Đọc tất cả các dòng từ file
                var lines = File.ReadAllLines(filePath);
                
                // Chuyển đổi dữ liệu sang format phù hợp
                var trainingData = lines.Select(line =>
                {
                    var parts = line.Split('\\');
                    return new DrinkEmotionData
                    {
                        CommentEmotional = parts[0].Trim(),
                        EmotionCategory = MapEmotionLabel(parts[1].Trim()),
                        Rating = 5, // Giá trị mặc định
                        Price = 0   // Giá trị mặc định
                    };
                }).ToList();

                return _mlContext.Data.LoadFromEnumerable(trainingData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc file training: {ex.Message}");
                throw;
            }
        }

        private string MapEmotionLabel(string label)
        {
            // Map nhãn số sang cảm xúc
            // Có thể điều chỉnh mapping này tùy theo dữ liệu của bạn
            switch (label)
            {
                case "0":
                    return "Negative";
                case "1":
                    return "Positive";
                default:
                    return label;
            }
        }

        private void ValidateTrainingData(string filePath)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                
                var lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
                Console.WriteLine($"Tổng số dòng ban đầu trong file: {lines.Length}");

                var uniqueLines = new HashSet<string>(lines);
                var duplicateCount = lines.Length - uniqueLines.Count;
                
                Console.WriteLine($"Số dòng trùng lặp: {duplicateCount}");
                Console.WriteLine($"Số dòng unique: {uniqueLines.Count}");

                var duplicates = lines.GroupBy(x => x)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => new { Line = g.Key, Count = g.Count() });

                foreach (var duplicate in duplicates)
                {
                    Console.WriteLine($"Dòng trùng lặp ({duplicate.Count} lần): {duplicate.Line}");
                }

                if (duplicateCount > 0)
                {
                    RemoveDuplicateLines(filePath);
                }

                lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
                Console.WriteLine($"Tổng số dòng sau khi xóa trùng lặp: {lines.Length}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi validate dữ liệu training: {ex.Message}");
                throw;
            }
        }

        private void RemoveDuplicateLines(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
                
                var uniqueLines = lines.Distinct().ToList();
                
                File.WriteAllLines(filePath, uniqueLines, System.Text.Encoding.UTF8);
                
                Console.WriteLine("Đã xóa các dòng trùng lặp thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa dòng trùng lặp: {ex.Message}");
                throw;
            }
        }
    }
}
