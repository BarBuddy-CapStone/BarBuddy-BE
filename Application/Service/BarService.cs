using Application.DTOs.Bar;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.IRepository;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Transactions;
using Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Application.Service
{
    public class BarService : IBarService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebase _fireBase;

        public BarService(IUnitOfWork unitOfWork, IMapper mapper, IFirebase fireBase)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBase = fireBase;
        }

        public async Task<BarResponse> CreateBar(BarRequest request)
        {
            var response = new BarResponse();
            string imageUrl = null;
            List<IFormFile> fileToUpload = new List<IFormFile>();
            List<string> listImgs = new List<string>();

            using (TransactionScope transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (!request.Images.IsNullOrEmpty())
                    {
                        foreach (var image in request.Images)
                        {
                            if (image.Length > 10 * 1024 * 1024)
                            {
                                throw new CustomException.InvalidDataException("File size exceeds the maximum allowed limit.");
                            }
                            fileToUpload.Add(image);
                        }
                    }

                    //Create với img là ""
                    var mapper = _mapper.Map<Bar>(request);
                    mapper.BarId = string.Format("{0}{1}", PrefixKeyConstant.BAR, Guid.NewGuid().ToString("N").ToUpper());
                    mapper.Images = "";
                    await _unitOfWork.BarRepository.InsertAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    foreach (var image in fileToUpload)
                    {
                        imageUrl = await _fireBase.UploadImageAsync(image);
                        listImgs.Add(imageUrl);
                    }

                    //Sau khi Upd ở trên thành công => lưu img lên firebase
                    var imagesAsString = string.Join(",", listImgs);
                    mapper.Images = imagesAsString;
                    await _unitOfWork.BarRepository.UpdateAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    transaction.Complete();
                    response = _mapper.Map<BarResponse>(mapper);

                }
                catch (Exception)
                {
                    transaction.Dispose();
                }
                return response;
            }
        }

        public async Task<IEnumerable<BarResponse>> GetAllBar()
        {
            var getAllBar = await _unitOfWork.BarRepository.GetAllAsync();

            if (getAllBar.IsNullOrEmpty() || !getAllBar.Any())
            {
                throw new CustomException.DataNotFoundException("The list is empty !");
            }
            var response = _mapper.Map<IEnumerable<BarResponse>>(getAllBar);

            return response;
        }

        public async Task<BarResponse> GetBarById(string barId)
        {
            var getBarById = await _unitOfWork.BarRepository.GetByIdAsync(barId);

            if (getBarById == null)
            {
                throw new CustomException.DataNotFoundException("Data not Found !");
            }

            var response = _mapper.Map<BarResponse>(getBarById);
            return response;
        }

        public async Task<BarResponse> UpdateBarById(string barId, BarRequest request)
        {
            var response = new BarResponse();
            string imageUrl = null;
            List<string> imgsList = new List<string>();
            List<IFormFile> imgsUpload = new List<IFormFile>();

            using (TransactionScope transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var getBarById = await _unitOfWork.BarRepository.GetByIdAsync(barId);

                    if (getBarById == null)
                    {
                        throw new CustomException.DataNotFoundException("Data not Found !");
                    }

                    if (!request.Images.IsNullOrEmpty())
                    {
                        foreach (var image in request.Images)
                        {
                            if (image.Length > 10 * 1024 * 1024)
                            {
                                throw new CustomException.InvalidDataException("File size exceeds the maximum allowed limit.");
                            }
                            imgsUpload.Add(image);
                        }
                    }

                    _mapper.Map(request, getBarById);
                    //Update với img là ""
                    getBarById.Images = "";
                    await _unitOfWork.BarRepository.UpdateAsync(getBarById);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();


                    foreach (var image in imgsUpload)
                    {
                        imageUrl = await _fireBase.UploadImageAsync(image);
                        imgsList.Add(imageUrl);
                    }

                    var imgsAsString = string.Join(",", imgsList);
                    //Sau khi Upd ở trên thành công => lưu img lên firebase
                    getBarById.Images = imgsAsString;
                    await _unitOfWork.BarRepository.UpdateAsync(getBarById);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    transaction.Complete();

                    response = _mapper.Map<BarResponse>(getBarById);
                }
                catch (CustomException.InternalServerErrorException e)
                {
                    transaction.Dispose();
                    throw new CustomException.InternalServerErrorException(e.Message);
                }
            }
            return response;
        }
    }
}
