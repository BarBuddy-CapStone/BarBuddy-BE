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
using Application.DTOs.DrinkCategory;
using Domain.Common;
using Azure.Core;
using System.Linq.Expressions;

namespace Application.Service
{
    public class DrinkService : IDrinkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebase _firebase;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthentication _authentication;

        public DrinkService(IUnitOfWork unitOfWork, IMapper mapper, 
                            IFirebase firebase, IAuthentication authentication, 
                            IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebase = firebase;
            _authentication = authentication;
            _contextAccessor = contextAccessor;
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

                    var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                    var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);
                    var getBar = _unitOfWork.BarRepository.Get(filter: x => x.BarId.Equals(request.BarId) &&
                                                                       x.Status == PrefixKeyConstant.TRUE)
                                                          .FirstOrDefault();
                    if (getBar == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy quán Bar");
                    }

                    if (getAccount.BarId != request.BarId)
                    {
                        throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                    }

                    var getDrinkCate = _unitOfWork.DrinkCategoryRepository
                                                    .Get(filter: x => x.DrinksCategoryId.Equals(request.DrinkCategoryId) &&
                                                                      x.IsDeleted == PrefixKeyConstant.FALSE)
                                                    .FirstOrDefault();

                    if(getDrinkCate == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy loại đồ uống !");
                    }

                    var isExistDrinkName = _unitOfWork.DrinkRepository
                                                        .Get(filter: x => x.DrinkName.Equals(request.DrinkName))
                                                        .FirstOrDefault();
                    if(isExistDrinkName != null)
                    {
                        throw new CustomException.DataExistException("Tên thức uống đã tồn tại ! Vui lòng thử lại.");
                    }

                    imgsFile = Utils.CheckValidateImageFile(request.Images);

                    var mapper = _mapper.Map<Drink>(request);
                    mapper.BarId = getBar.BarId;
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
                            .Get(e => e.EmotionalDrinksCategoryId.Equals(emotion) && 
                                      e.IsDeleted == PrefixKeyConstant.FALSE)
                            .FirstOrDefault();
                        if (emotionid == null)
                        {
                            throw new CustomException.DataNotFoundException("Không tìm thấy cảm xúc !");
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
                    response.DrinkCategoryResponse = _mapper.Map<DrinkCategoryResponse>(getDrinkCate);
                }
                catch (CustomException.InvalidDataException e)
                {
                    transaction.Dispose();
                    throw new CustomException.InvalidDataException(e.Message);
                }
                return response;
            }
        }

        public async Task<IEnumerable<DrinkResponse>> GetAllDrink(QueryDrinkRequest query)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                Expression<Func<Drink, bool>> filter = drink =>
                        drink.BarId.Equals(getAccount.BarId) &&
                        (string.IsNullOrWhiteSpace(query.Search) || drink.DrinkName.Contains(query.Search)) &&
                        (!query.cateId.HasValue || drink.DrinkCategoryId.Equals(query.cateId));

                var getAllDrink = await _unitOfWork.DrinkRepository
                    .GetAsync(
                        filter: filter,
                        includeProperties: "Bar,DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory"
                    );
                if (getAllDrink.IsNullOrEmpty())
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy đồ uống !");
                }
                var response = _mapper.Map<IEnumerable<DrinkResponse>>(getAllDrink);
                return response;
            }
            catch(CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
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
        public async Task<PagingDrinkResponse> GetAllDrinkBasedCateId(Guid cateId, ObjectQueryCustom query)
        {
            try
            {
                var pageIndex = query.PageIndex ?? 1;
                var pageSize = query.PageSize ?? 6;

                var getAllDrink = await _unitOfWork.DrinkRepository
                                    .GetAsync(filter: x => (string.IsNullOrWhiteSpace(query.Search) || query.Search.Contains(x.DrinkName)) && 
                                                            x.DrinkCategory.DrinksCategoryId.Equals(cateId)
                                    , includeProperties: "DrinkCategory,Bar");

                if (getAllDrink.IsNullOrEmpty())
                {
                    throw new CustomException.DataNotFoundException("Data not found !");
                }

                var response = _mapper.Map<List<DrinkResponse>>(getAllDrink);

                var totalItems = response.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var paginatedDrinks = response.Skip((pageIndex - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToList();

                return new PagingDrinkResponse
                {
                    DrinkResponses = paginatedDrinks,
                    TotalPages = totalPages,
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
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
                    var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                    var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);
                    var getBar = _unitOfWork.BarRepository.Get(filter: x => x.BarId.Equals(request.BarId) &&
                                                                       x.Status == PrefixKeyConstant.TRUE)
                                                          .FirstOrDefault();
                    var getDrinkCate = _unitOfWork.DrinkCategoryRepository
                                                    .Get(filter: x => x.DrinksCategoryId.Equals(request.DrinkCategoryId) && 
                                                                      x.IsDeleted == PrefixKeyConstant.FALSE)
                                                    .FirstOrDefault();
                    
                    if (getBar == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy quán Bar");
                    }

                    if (getAccount.BarId != request.BarId)
                    {
                        throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                    }

                    if (!request.Images.IsNullOrEmpty())
                    {
                        imgsFile = Utils.CheckValidateImageFile(request.Images);
                    }
                    var getOneDrink = (await _unitOfWork.DrinkRepository
                                                        .GetAsync(filter: x => x.DrinkId.Equals(drinkId) && 
                                                                               x.BarId.Equals(getBar.BarId)))
                                                        .FirstOrDefault();

                    if (getOneDrink == null)
                    {
                        throw new CustomException.DataNotFoundException("Data not found !");
                    }

                    if (request.OldImages.IsNullOrEmpty() && getOneDrink.Image.IsNullOrEmpty())
                    {
                        throw new CustomException.InvalidDataException("Invalid data");
                    }

                    if (getDrinkCate == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy loại đồ uống !");
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
                        var isExistEmo = _unitOfWork.EmotionalDrinkCategoryRepository
                                                    .Get(filter: x => x.EmotionalDrinksCategoryId.Equals(emotionToAdd) &&
                                                                      x.IsDeleted == PrefixKeyConstant.FALSE).FirstOrDefault();
                        if (isExistEmo == null)
                        {
                            throw new CustomException.DataNotFoundException("Không tìm thấy cảm xúc đồ uống !");
                        }
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
                                                    drink.DrinkId, random.Next(5), random);
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
                            if (breakPoint >= 180) throw new InvalidDataException("BreakPoint");
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
                    : 10000000;
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
