using Application.Common;
using Application.DTOs.Drink;
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
                    imgsFile = Utils.CheckValidateImageFile(request.Images);

                    var mapper = _mapper.Map<Drink>(request);
                    mapper.DrinkCode = PrefixKeyConstant.DRINK;
                    mapper.Image = "";
                    await _unitOfWork.DrinkRepository.InsertAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

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
                var getAllDrink = await _unitOfWork.DrinkRepository.GetAsync(includeProperties: "DrinkCategory,Bar");
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
                                                includeProperties: "DrinkCategory,Bar");
                var getOne = getAllDrink.FirstOrDefault();

                if (getOne == null)
                {
                    throw new CustomException.DataNotFoundException("Data not found !");
                }
                var response = _mapper.Map<DrinkResponse>(getOne);
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

            using (TransactionScope transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var getOneDrink = await _unitOfWork.DrinkRepository.GetByIdAsync(drinkId);

                    if (getOneDrink == null)
                    {
                        throw new CustomException.DataNotFoundException("Data not found !");
                    }

                    imgsFile = Utils.CheckValidateImageFile(request.Images);

                    var mapper = _mapper.Map(request, getOneDrink);
                    mapper.Image = "";

                    await _unitOfWork.DrinkRepository.UpdateAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

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
                    //Cap nhat lai data da upd
                    var getAllDrink = await _unitOfWork.DrinkRepository
                                        .GetAsync(filter: x => x.DrinkId.Equals(drinkId),
                                                includeProperties: "DrinkCategory,Bar");
                    var getOne = getAllDrink.FirstOrDefault();
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
    }
}
