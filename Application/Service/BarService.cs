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
using Application.Common;
using Domain.Enums;
using Domain.Utils;
using Domain.Common;
using System.Linq.Expressions;

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
                    if (request.Images.IsNullOrEmpty())
                    {
                        throw new CustomException.InvalidDataException("Invalid data");
                    }

                    fileToUpload = Utils.CheckValidateImageFile(request.Images);

                    //Create với img là ""
                    var mapper = _mapper.Map<Bar>(request);
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

        public async Task<IEnumerable<BarResponse>> GetAllBarWithFeedback(ObjectQuery query)
        {

            Expression<Func<Bar, bool>> filter = null;

            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                filter = barName => barName.BarName.Contains(query.Search);
            }

            var bars = await _unitOfWork.BarRepository
                                    .GetAsync(filter: filter,
                                                        pageIndex: query.PageIndex, 
                                                        pageSize: query.PageSize,
                                                        includeProperties: "Feedbacks");

            var currentDateTime = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);
            var response = new List<BarResponse>();



            if (bars.IsNullOrEmpty() || !bars.Any())
            {
                throw new CustomException.DataNotFoundException("The list is empty !");
            }

            foreach (var bar in bars)
            {
                var tables = await _unitOfWork.TableRepository
                                                .GetAsync(t => t.BarId == bar.BarId && t.IsDeleted == false);
                bool isAnyTableAvailable = false;
                foreach (var table in tables)
                {
                    var reservations = await _unitOfWork.BookingTableRepository
                                                        .GetAsync(bt => bt.TableId == table.TableId &&
                                                                         (bt.ReservationDate + bt.ReservationTime) >= currentDateTime &&
                                                                         (bt.Booking.Status == (int)PrefixValueEnum.Pending ||
                                                                            bt.Booking.Status == (int)PrefixValueEnum.Serving));

                    if (!reservations.Any())
                    {
                        isAnyTableAvailable = true;
                        break;
                    }
                }
                var mapper = _mapper.Map<BarResponse>(bar);
                mapper.IsAnyTableAvailable = isAnyTableAvailable;
                response.Add(mapper);
            }
            return response;
        }

        public async Task<BarResponse> GetBarById(Guid barId)
        {
            var getBarById = await _unitOfWork.BarRepository.GetByIdAsync(barId);

            if (getBarById == null)
            {
                throw new CustomException.DataNotFoundException("Data not Found !");
            }

            var response = _mapper.Map<BarResponse>(getBarById);
            return response;
        }

        public async Task<BarResponse> GetBarByIdWithFeedback(Guid barId)
        {
            bool isAnyTableAvailable = false;
            var response = new BarResponse();
            var currentDateTime = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);
            var getBarById = (await _unitOfWork.BarRepository.GetAsync(filter: a => a.BarId == barId,
                    includeProperties: "Feedbacks")).FirstOrDefault();

            if (getBarById == null)
            {
                throw new CustomException.DataNotFoundException("Data not Found !");
            }

            var tables = await _unitOfWork.TableRepository
                                                .GetAsync(t => t.BarId == getBarById.BarId && t.IsDeleted == false);

            foreach (var table in tables)
            {
                var reservations = await _unitOfWork.BookingTableRepository
                                                    .GetAsync(bt => bt.TableId == table.TableId &&
                                                                     (bt.ReservationDate + bt.ReservationTime) >= currentDateTime &&
                                                                     (bt.Booking.Status == (int)PrefixValueEnum.Pending ||
                                                                        bt.Booking.Status == (int)PrefixValueEnum.Serving));

                if (!reservations.Any())
                {
                    isAnyTableAvailable = true;
                    break;
                }
            }

            response = _mapper.Map<BarResponse>(getBarById);
            response.IsAnyTableAvailable = isAnyTableAvailable;
            return response;
        }

        public async Task<BarResponse> GetBarByIdWithTable(Guid barId)
        {
            var getBarById = (await _unitOfWork.BarRepository.GetAsync(filter: a => a.BarId == barId,
                    includeProperties: "Tables,Tables.TableType")).FirstOrDefault();

            if (getBarById == null)
            {
                throw new CustomException.DataNotFoundException("Data not Found !");
            }

            var response = _mapper.Map<BarResponse>(getBarById);
            return response;
        }

        public async Task<BarResponse> UpdateBarById(Guid barId, BarRequest request)
        {
            var response = new BarResponse();
            string imageUrl = null;
            string imgsUploaed = string.Empty;
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
                        imgsUpload = Utils.CheckValidateImageFile(request.Images);
                    }

                    if (request.imgsAsString.IsNullOrEmpty() && getBarById.Images.IsNullOrEmpty())
                    {
                        throw new CustomException.InvalidDataException("Invalid data");
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
                    imgsUploaed = string.Join(",", request.imgsAsString);

                    var allImg = string.IsNullOrEmpty(imgsAsString) ? imgsUploaed : $"{imgsUploaed},{imgsAsString}";
                    //Sau khi Upd ở trên thành công => lưu img lên firebase
                    getBarById.Images = allImg;
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
