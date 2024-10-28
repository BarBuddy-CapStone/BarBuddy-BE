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
                                                includeProperties: "DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory");
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
                                                includeProperties: "DrinkCategory,DrinkEmotionalCategories.EmotionalDrinkCategory");
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
    }
}
