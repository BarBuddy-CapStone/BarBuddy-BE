using Application.DTOs.PaymentHistory;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Domain.CustomException.CustomException;

namespace Application.Service
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthentication _authentication;
        private readonly IHttpContextAccessor _contextAccessor;

        public PaymentHistoryService(IUnitOfWork unitOfWork, IMapper mapper, 
                                        IAuthentication authentication, 
                                        IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authentication = authentication;
            _contextAccessor = contextAccessor;
        }

        public async Task<(List<PaymentHistoryResponse> response, int totalPage)> Get(int Status, string? CustomerName, string? PhoneNumber, string? Email, Guid? BarId, DateTime? PaymentDate, int PageIndex, int PageSize)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                if (getAccount.BarId.HasValue)
                {
                    if (BarId.HasValue && !getAccount.BarId.Equals(BarId))
                    {
                        throw new UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này!");
                    }
                    BarId = getAccount.BarId;
                }

                var response = new List<PaymentHistoryResponse>();

                // filter expression
                Expression<Func<PaymentHistory, bool>> filter = p =>
                p.Status == Status &&
                (string.IsNullOrEmpty(CustomerName) || p.Account.Fullname.Contains(CustomerName)) &&
                (string.IsNullOrEmpty(PhoneNumber) || p.Account.Phone.Contains(PhoneNumber)) &&
                (string.IsNullOrEmpty(Email) || p.Account.Email.Contains(Email)) &&
                (!BarId.HasValue || p.Booking.BarId == BarId.Value) &&
                (!PaymentDate.HasValue || p.PaymentDate.Date == PaymentDate.Value.Date);

                var payments = await _unitOfWork.PaymentHistoryRepository.GetAsync(filter: filter);

                if (payments is null || !payments.Any())
                {
                    throw new CustomException.DataNotFoundException("Bạn không có lịch sử giao dịch nào cả !");
                }

                int totalPage = 1;
                if (payments.Count() > PageSize)
                {
                    if(PageSize == 1)
                    {
                        totalPage = (payments.Count() / PageSize);
                    } else
                    {
                        totalPage = (payments.Count() / PageSize) + 1;
                    }
                }

                var paymentsWithPagination = await _unitOfWork.PaymentHistoryRepository.GetAsync(filter: filter, includeProperties: "Account,Booking", pageIndex: PageIndex, pageSize: PageSize);
                
                if(paymentsWithPagination is null )
                {
                    throw new CustomException.DataNotFoundException("Bạn không có lịch sử giao dịch nào cả !");
                }

                foreach (var payment in paymentsWithPagination) {
                    var paymentResponse = new PaymentHistoryResponse
                    {
                        CustomerName = payment.Account.Fullname,
                        PhoneNumber = payment.Account.Phone,
                        BarName = (await _unitOfWork.BarRepository.GetByIdAsync(payment.Booking.BarId)).BarName,
                        TransactionCode = payment.TransactionCode,
                        ProviderName = payment.ProviderName,
                        Status = payment.Status,
                        Note = payment.Note,
                        PaymentDate = payment.PaymentDate,
                        TotalPrice = payment.TotalPrice
                    };

                    response.Add(paymentResponse);
                }

                return (response, totalPage);
            }
            catch (UnAuthorizedException ex)
            {
                throw new UnAuthorizedException(ex.Message);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex) 
            { 
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<PaymentHistoryByCustomerResponse> response, int totalPage)> GetByCustomerId(Guid customerId, int? Status, int PageIndex, int PageSize)
        {
            try
            {

                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                if(!accountId.Equals(customerId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào tài khoản này !");
                }
                var response = new List<PaymentHistoryByCustomerResponse>();
                int totalPage = 1;

                // *** find role id temporarily
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "CUSTOMER")).FirstOrDefault();
                if (role == null)
                {
                    throw new CustomException.InternalServerErrorException("Cannot find role");
                }

                var customer = await _unitOfWork.AccountRepository.GetAsync(a => a.AccountId == accountId && a.RoleId == role.RoleId);
                if(customer == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy khách hàng");
                }
                
                var payments = await _unitOfWork.PaymentHistoryRepository.GetAsync(filter: p => (Status == null || p.Status == Status) && p.AccountId == accountId);

                if (payments.Count() > PageSize)
                {
                    if (PageSize == 1)
                    {
                        totalPage = (payments.Count() / PageSize);
                    }
                    else
                    {
                        totalPage = (payments.Count() / PageSize) + 1;
                    }
                }

                var paymentsWithPagination = await _unitOfWork.PaymentHistoryRepository.GetAsync(filter: p => (Status == null || p.Status == Status) && p.AccountId == accountId, includeProperties: "Account,Booking", pageIndex: PageIndex, pageSize: PageSize);

                foreach (var payment in paymentsWithPagination)
                {
                    var paymentResponse = new PaymentHistoryByCustomerResponse
                    {
                        BookingId = payment.BookingId,
                        CustomerName = payment.Account.Fullname,
                        PhoneNumber = payment.Account.Phone,
                        BarName = (await _unitOfWork.BarRepository.GetByIdAsync(payment.Booking.BarId)).BarName,
                        TransactionCode = payment.TransactionCode,
                        Status = payment.Status,
                        Note = payment.Note,
                        PaymentDate = payment.PaymentDate,
                        TotalPrice = payment.TotalPrice,
                        ProviderName = payment.ProviderName,
                        PaymentFee = payment.PaymentFee
                    };

                    response.Add(paymentResponse);
                }

                return (response, totalPage);
            }
            catch (UnAuthorizedException ex)
            {
                throw new UnAuthorizedException(ex.Message);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex) { 
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<PaymentHistoryResponse> response, int totalPage)> GetByBarId(Guid barId, int pageIndex, int pageSize)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                if (!getAccount.BarId.Equals(barId))
                {
                    throw new UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                var payments = await _unitOfWork.PaymentHistoryRepository
                    .GetAsync(filter: p => p.Booking.BarId.Equals(barId),
                    includeProperties: "Booking.Bar,Account");

                if (payments == null || !payments.Any())
                {
                    return (new List<PaymentHistoryResponse>(), 0);
                }

                int totalPage = 1;
                int validPageIndex = pageIndex > 0 ? pageIndex - 1 : 0;
                int validPageSize = pageSize > 0 ? pageSize : 10;
                if (payments.Count() > pageSize)
                {
                    if (pageSize == 1)
                    {
                        totalPage = (payments.Count() / pageSize);
                    }
                    else
                    {
                        totalPage = (payments.Count() / pageSize) + 1;
                    }
                }
                var paginationPayments = payments.Skip(validPageIndex * validPageSize).Take(validPageSize);
                return (_mapper.Map<List<PaymentHistoryResponse>>(paginationPayments), totalPage);
            }
            catch (UnAuthorizedException ex)
            {
                throw new UnAuthorizedException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
