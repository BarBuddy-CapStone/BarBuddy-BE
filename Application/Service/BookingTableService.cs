using Application.DTOs.BookingTable;
using Application.DTOs.Table;
using Application.DTOs.TableType;
using Application.IService;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Domain.Utils;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class BookingTableService : IBookingTableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingTableService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FilterTableTypeReponse> FilterTableTypeReponse(FilterTableDateTimeRequest request)
        {
            try
            {
                if(TimeHelper.ConvertToUtcPlus7(request.Date.Date) <= TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now))
                {
                    throw new CustomException.InvalidDataException("Invalid data");
                }

                var data = await _unitOfWork.TableRepository.GetAsync(
                                                filter: x => x.BarId.Equals(request.BarId)
                                                          && x.TableTypeId.Equals(request.TableTypeId)
                                                          && x.IsDeleted == PrefixKeyConstant.FALSE,
                                                includeProperties: "BookingTables.Booking,TableType,Bar");

                if(data.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Invalid data");
                }

                var getOne = data.Where(x => x.BookingTables != null && x.BookingTables.Any()).FirstOrDefault();


                var response = _mapper.Map<FilterTableTypeReponse>(getOne.TableType);

                response.BookingTables = new List<FilterBkTableResponse>
                {
                    new FilterBkTableResponse
                    {
                        ReservationDate = getOne.BookingTables.FirstOrDefault().ReservationDate,
                        ReservationTime = getOne.BookingTables.FirstOrDefault().ReservationTime,
                        Tables = data.Select(bt => new FilterTableResponse
                                    {
                                        TableId = bt.TableId,
                                        TableName = bt.TableName,
                                        Status = bt.BookingTables != null && bt.BookingTables.Any()
                                                ? bt.BookingTables.FirstOrDefault()?.Booking.Status ?? 0
                                                : 0
                                    }).ToList()
                    }
                };

                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException("Đã xảy ra lỗi");
            }
        }
    }
}
