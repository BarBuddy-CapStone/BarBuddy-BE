using Application.Common;
using Application.DTOs.Drink;
using Application.DTOs.DrinkEmoCate;
using Application.DTOs.Response.EmotionCategory;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Transactions;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net.Http;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Application.DTOs.ML;
using CsvHelper.Configuration;
using CsvHelper;

namespace Application.Service
{
    public class DrinkService : IDrinkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebase _firebase;

        public DrinkService(IUnitOfWork unitOfWork, IMapper mapper, IFirebase firebase)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebase = firebase;
        }

        public async Task<DrinkResponse> CreateDrink(DrinkRequest request)
        {
            var response = new DrinkResponse();
            List<IFormFile> imgsFile = new List<IFormFile>();
            List<string> imgsString = new List<string>();
            string imgsAsString = string.Empty;


            using (TransactionScope transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (request.Images.IsNullOrEmpty())
                    {
                        throw new CustomException.InvalidDataException("Invalid data !");
                    }

                    imgsFile = Utils.CheckValidateImageFile(request.Images);

                    var mapper = _mapper.Map<Drink>(request);

                    mapper.DrinkCode = PrefixKeyConstant.DRINK;
                    mapper.Image = "";
                    mapper.Status = PrefixKeyConstant.TRUE;
                    mapper.CreatedDate = DateTime.UtcNow;
                    mapper.UpdatedDate = mapper.CreatedDate;

                    await _unitOfWork.DrinkRepository.InsertAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    foreach (var emotion in request.DrinkBaseEmo)
                    {
                        var emotionid = _unitOfWork.EmotionalDrinkCategoryRepository
                            .Get(e => e.EmotionalDrinksCategoryId.Equals(emotion))
                            .FirstOrDefault();
                        if (emotionid == null)
                        {
                            throw new CustomException.DataNotFoundException("Data not found");
                        }

                        var drinkEmotionalCategory = new DrinkEmoCateRequest
                        {
                            DrinkId = mapper.DrinkId,
                            EmotionalDrinkCategoryId = emotion
                        };

                        var mp = _mapper.Map<DrinkEmotionalCategory>(drinkEmotionalCategory);

                        await _unitOfWork.DrinkEmotionalCategoryRepository.InsertAsync(mp);
                        await Task.Delay(200);
                        await _unitOfWork.SaveAsync();
                    }

                    foreach (var image in imgsFile)
                    {
                        var uploadImg = await _firebase.UploadImageAsync(image);
                        imgsString.Add(uploadImg);
                    }

                    imgsAsString = string.Join(",", imgsString);
                    mapper.Image = imgsAsString;

                    await _unitOfWork.DrinkRepository.UpdateAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    transaction.Complete();
                    response = _mapper.Map<DrinkResponse>(mapper);
                }
                catch (CustomException.InvalidDataException e)
                {
                    transaction.Dispose();
                    throw new CustomException.InvalidDataException(e.Message);
                }
                return response;
            }
        }

        public async Task<IEnumerable<DrinkResponse>> GetAllDrink()
        {
            try
            {
                var getAllDrink = await _unitOfWork.DrinkRepository.GetAsync(includeProperties: "DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory");
                if (getAllDrink.IsNullOrEmpty())
                {
                    throw new CustomException.DataNotFoundException("Data not found !");
                }
                var response = _mapper.Map<IEnumerable<DrinkResponse>>(getAllDrink);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
        public async Task<IEnumerable<DrinkResponse>> GetAllDrinkCustomer()
        {
            try
            {
                var getAllDrink = await _unitOfWork.DrinkRepository
                                        .GetAsync(filter: x => x.Status == PrefixKeyConstant.TRUE,
                                                includeProperties: "DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory,Bar");
                var response = _mapper.Map<IEnumerable<DrinkResponse>>(getAllDrink);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
        }
        public async Task<IEnumerable<DrinkResponse>> GetDrinkCustomerOfBar(Guid barId)
        {
            try
            {
                var getAllDrink = await _unitOfWork.DrinkRepository
                                        .GetAsync(filter: x => x.Status == PrefixKeyConstant.TRUE && x.Bar.BarId.Equals(barId),
                                                includeProperties: "DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory,Bar");
                var response = _mapper.Map<IEnumerable<DrinkResponse>>(getAllDrink);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
        }
        public async Task<IEnumerable<DrinkResponse>> GetAllDrinkBasedCateId(Guid cateId)
        {
            try
            {
                var getAllDrink = await _unitOfWork.DrinkRepository
                                    .GetAsync(filter: x => x.DrinkCategory.DrinksCategoryId.Equals(cateId)
                                    , includeProperties: "DrinkCategory");

                if (getAllDrink.IsNullOrEmpty())
                {
                    throw new CustomException.DataNotFoundException("Data not found !");
                }
                var response = _mapper.Map<IEnumerable<DrinkResponse>>(getAllDrink);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
        }

        public async Task<DrinkResponse> GetDrink(Guid drinkId)
        {
            try
            {
                var getAllDrink = await _unitOfWork.DrinkRepository
                                        .GetAsync(filter: x => x.DrinkId.Equals(drinkId),
                                                includeProperties: "Bar,DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory");
                var getOne = getAllDrink.FirstOrDefault();

                if (getOne == null)
                {
                    throw new CustomException.DataNotFoundException("Data not found !");
                }

                var response = _mapper.Map<DrinkResponse>(getOne);

                response.EmotionsDrink = getOne.DrinkEmotionalCategories
                    .Select(dec => new EmotionCategoryResponse
                    {
                        EmotionalDrinksCategoryId = dec.EmotionalDrinkCategoryId,
                        CategoryName = dec.EmotionalDrinkCategory.CategoryName
                    }).ToList();
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
        }

        public async Task<DrinkResponse> UpdateDrink(Guid drinkId, DrinkRequest request)
        {
            var response = new DrinkResponse();
            List<IFormFile> imgsFile = new List<IFormFile>();
            List<string> imgsString = new List<string>();
            string imgsAsString = string.Empty;
            string oldImgsUploaded = string.Empty;

            using (TransactionScope transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (!request.Images.IsNullOrEmpty())
                    {
                        imgsFile = Utils.CheckValidateImageFile(request.Images);
                    }
                    var getOneDrink = await _unitOfWork.DrinkRepository.GetByIdAsync(drinkId);

                    if (getOneDrink == null)
                    {
                        throw new CustomException.DataNotFoundException("Data not found !");
                    }

                    if (request.OldImages.IsNullOrEmpty() && getOneDrink.Image.IsNullOrEmpty())
                    {
                        throw new CustomException.InvalidDataException("Invalid data");
                    }


                    var mapper = _mapper.Map(request, getOneDrink);
                    mapper.Image = "";
                    mapper.UpdatedDate = DateTime.Now;

                    var existEmotion = await _unitOfWork.DrinkEmotionalCategoryRepository
                                    .GetAsync(filter: x => x.Drink.DrinkId.Equals(getOneDrink.DrinkId));

                    var newEmotions = request.DrinkBaseEmo;
                    var emotionsToDelete = existEmotion
                                            .Where(existing => !newEmotions.Contains(existing.EmotionalDrinkCategoryId))
                                            .ToList();

                    foreach (var emotionToDelete in emotionsToDelete)
                    {
                        await _unitOfWork.DrinkEmotionalCategoryRepository
                            .DeleteAsync(emotionToDelete.DrinkEmotionalCategoryId);
                    }

                    var emotionsToAdd = newEmotions
                        .Where(newEmotion => !existEmotion.Any(existing => existing.EmotionalDrinkCategoryId.Equals(newEmotion)))
                        .ToList();

                    foreach (var emotionToAdd in emotionsToAdd)
                    {
                        var newEmotion = new DrinkEmotionalCategory
                        {
                            DrinkId = mapper.DrinkId,
                            EmotionalDrinkCategoryId = emotionToAdd
                        };
                        await _unitOfWork.DrinkEmotionalCategoryRepository.InsertAsync(newEmotion);
                    }

                    await _unitOfWork.SaveAsync();

                    await _unitOfWork.DrinkRepository.UpdateAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    foreach (var image in imgsFile)
                    {
                        var uploadImg = await _firebase.UploadImageAsync(image);
                        imgsString.Add(uploadImg);
                    }

                    imgsAsString = string.Join(",", imgsString);
                    oldImgsUploaded = string.Join(",", request.OldImages);

                    var allImg = string.IsNullOrEmpty(imgsAsString) ? oldImgsUploaded : $"{oldImgsUploaded},{imgsAsString}";

                    mapper.Image = allImg;
                    await _unitOfWork.DrinkRepository.UpdateAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    

                    var getAllDrink = await _unitOfWork.DrinkRepository
                                        .GetAsync(filter: x => x.DrinkId.Equals(mapper.DrinkId),
                                                includeProperties: "DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory");
                    var getOne = getAllDrink.FirstOrDefault();
                    transaction.Complete();
                    response = _mapper.Map<DrinkResponse>(getOne);
                    return response;
                }
                catch (CustomException.InternalServerErrorException e)
                {
                    transaction.Dispose();
                    throw new CustomException.InternalServerErrorException(e.Message);
                }
            }
        }

        public async Task<IEnumerable<DrinkResponse>> GetAllDrinkBasedEmoId(Guid emoId)
        {
            try
            {
                var getAllDrink = await _unitOfWork.DrinkRepository.GetAsync(includeProperties: "DrinkCategory");

                if (getAllDrink.IsNullOrEmpty())
                {
                    throw new CustomException.DataNotFoundException("Data not found !");
                }
                var response = _mapper.Map<IEnumerable<DrinkResponse>>(getAllDrink);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
        }

        public async Task<string> CrawlDrink()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HtmlDocument homepageDoc = new HtmlDocument();
                var drinkCategory = (await _unitOfWork.DrinkCategoryRepository.GetAsync(x => x.DrinksCategoryName == "Rượu mạnh")).First();
                var bars = _unitOfWork.BarRepository.GetAll().ToArray();

                string[] childUrls = ["whisky", "vang-champagne", "spirits-liqueur", "sake-beer"];
                int totalCrawled = 0;
                var random = new Random();

                foreach (var childUrl in childUrls)
                {
                    try 
                    {
                        string baseUrl = $"https://sanhruou.com/{childUrl}.html";
                        Console.WriteLine($"Đang crawl category: {childUrl}");

                        var initialResponse = await httpClient.GetStringAsync(baseUrl);
                        homepageDoc.LoadHtml(initialResponse);
                        
                        var productCountNode = homepageDoc.DocumentNode
                            .SelectSingleNode("//b[contains(@class, 'page-count-total')]");
                        
                        if (productCountNode == null) continue;

                        string productCountText = productCountNode.InnerText.Replace(".", "");
                        if (!int.TryParse(productCountText, out int totalProducts))
                        {
                            continue;
                        }

                        int productsPerPage = 30;
                        int totalPages = (int)Math.Ceiling((double)totalProducts / productsPerPage);
                        Console.WriteLine($"Tổng số trang cần crawl: {totalPages}");

                        for (int page = 1; page <= totalPages; page++)
                        {
                            try
                            {
                                string pageUrl = page == 1 ? baseUrl : $"{baseUrl}?i={page}";
                                Console.WriteLine($"Đang crawl trang {page}/{totalPages}: {pageUrl}");

                                var pageResponse = await httpClient.GetStringAsync(pageUrl);
                                homepageDoc.LoadHtml(pageResponse);

                                var productLinks = homepageDoc.DocumentNode
                                    .SelectNodes("//a[contains(@class, 'art-card no-rating')]");

                                if (productLinks != null)
                                {
                                    List<Drink> pageDrinks = new List<Drink>();
                                    int j = 0;
                                    foreach (var link in productLinks)
                                    {
                                        try
                                        {
                                            if (j >= 10) j = 0;
                                            string productUrl = $"https://sanhruou.com/{link.Attributes["href"].Value}";
                                            var drink = await CrawlSingleDrink(httpClient, productUrl, drinkCategory.DrinksCategoryId, bars[j].BarId);
                                            if (drink != null)
                                            {
                                                drink.DrinkEmotionalCategories = RandomDrinkEmotionalCategories(
                                                    drink.DrinkId, random.Next(6), random);
                                                pageDrinks.Add(drink);
                                                totalCrawled++;
                                                j++;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Lỗi crawl sản phẩm: {ex.Message}");
                                            continue;
                                        }
                                    }

                                    if (pageDrinks.Any())
                                    {
                                        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                        {
                                            try
                                            {
                                                _unitOfWork.DrinkRepository.InsertRange(pageDrinks);
                                                await _unitOfWork.SaveAsync();
                                                transaction.Complete();
                                                Console.WriteLine($"Đã lưu {pageDrinks.Count} sản phẩm từ trang {page}");
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"Lỗi lưu dữ liệu trang {page}: {ex.Message}");
                                            }
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Lỗi crawl trang {page}: {ex.Message}");
                                continue;
                            }
                            var breakPoint = _unitOfWork.DrinkRepository.GetAll().Count();
                            if (breakPoint >= 300) throw new InvalidDataException("BreakPoint");
                        }
                    }
                    catch (InvalidDataException ix)
                    {
                        throw new InvalidDataException(ix.Message);
                    } 
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi crawl category {childUrl}: {ex.Message}");
                        continue;
                    }
                }

                return $"Crawl thành công {totalCrawled} sản phẩm";
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }

        private async Task<Drink> CrawlSingleDrink(HttpClient httpClient, string productUrl, Guid categoryId, Guid barId)
        {
            try
            {
                var productResponse = await httpClient.GetStringAsync(productUrl);
                HtmlDocument productDoc = new HtmlDocument();
                productDoc.LoadHtml(productResponse);

                var productNameNode = productDoc.DocumentNode
                    .SelectSingleNode("//h1[contains(@class, 'pd-name')]");
                var descriptionNode = productDoc.DocumentNode
                    .SelectSingleNode("//div[contains(@itemprop, 'description')]");
                var priceNode = productDoc.DocumentNode
                    .SelectSingleNode("//div[contains(@class, 'pd-price')]/meta[@itemprop='price']");
                var imgNode = productDoc.DocumentNode
                    .SelectSingleNode("//img[contains(@class, 'gal-item-content file-img')]");

                if (productNameNode == null) return null;

                string name = productNameNode.InnerText.Trim();
                string description = descriptionNode?.InnerText?.Trim() ?? "Chưa có mô tả";
                string priceText = priceNode?.GetAttributeValue("content", "0") ?? "0";
                string image = imgNode?.GetAttributeValue("src", "") ?? "";

                string cleanedPrice = Regex.Replace(priceText, @"[^\d\.]", "");

                if (cleanedPrice.EndsWith(".00"))
                {
                    cleanedPrice = cleanedPrice.Substring(0, cleanedPrice.Length - 3);
                }

                double price = double.TryParse(cleanedPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedPrice)
                    ? parsedPrice
                    : 0;
                return new Drink
                {
                    DrinkName = name.Trim(),
                    BarId = barId,
                    Image = $"https:{image}",
                    Price = price,
                    Description = description.Trim(),
                    DrinkCode = PrefixKeyConstant.DRINK,
                    DrinkCategoryId = categoryId,
                    Status = PrefixKeyConstant.TRUE,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
            }
            catch
            {
                return null;
            }
        }

        private HashSet<DrinkEmotionalCategory> RandomDrinkEmotionalCategories(Guid drinkId, int count, 
            Random random)
        {
            var emotions = _unitOfWork.EmotionalDrinkCategoryRepository.GetAll().ToList();

            var drinkEmotionalCategories = new HashSet<DrinkEmotionalCategory>();

            count = Math.Min(count, emotions.Count);

            for (int i = 0; i < count; i++)
            {
                var randomEmotion = emotions.OrderBy(x => random.Next()).First();

                var drinkEmotionalCategory = new DrinkEmotionalCategory
                {
                    DrinkId = drinkId,
                    EmotionalDrinkCategoryId = randomEmotion.EmotionalDrinksCategoryId
                };

                drinkEmotionalCategories.Add(drinkEmotionalCategory);

                emotions.Remove(randomEmotion);
            }

            return drinkEmotionalCategories;
        }


        public async Task<IEnumerable<DrinkResponse>> GetAllDrinkCustomerOfBar(Guid barId)
        {
            try
            {
                var getAllDrink = await _unitOfWork.DrinkRepository
                                        .GetAsync(filter: x => x.Status == PrefixKeyConstant.TRUE /*&&
                                                x.DrinkCategory.BarId.Equals(barId)*/,
                                                includeProperties: "DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory");
                var response = _mapper.Map<IEnumerable<DrinkResponse>>(getAllDrink);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
        }
    }
}
