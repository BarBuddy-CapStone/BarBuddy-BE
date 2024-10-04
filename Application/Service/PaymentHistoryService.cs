using Application.DTOs.PaymentHistory;
using Application.IService;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentHistoryService(IUnitOfWork unitOfWork) { 
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<PaymentHistoryResponse> response, int totalPage)> Get(bool Status, string? CustomerName, string? PhoneNumber, string? Email, Guid? BarId, DateTime? PaymentDate, int PageIndex, int PageSize)
        {
            try
            {
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

                foreach (var payment in paymentsWithPagination) {
                    var paymentResponse = new PaymentHistoryResponse
                    {
                        CustomerName = payment.Account.Fullname,
                        PhoneNumber = payment.Account.Phone,
                        BarName = (await _unitOfWork.BarRepository.GetByIdAsync(payment.Booking.BarId)).BarName,
                        TransactionCode = payment.TransactionCode,
                        Status = payment.Status,
                        Note = payment.Note,
                        PaymentDate = payment.PaymentDate,
                        TotalPrice = payment.TotalPrice
                    };

                    response.Add(paymentResponse);
                }

                return (response, totalPage);
            }
            catch (Exception ex) { 
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<PaymentHistoryByCustomerResponse> response, int totalPage)> GetByCustomerId(Guid customerId, bool? Status, int PageIndex, int PageSize)
        {
            try
            {
                var response = new List<PaymentHistoryByCustomerResponse>();
                int totalPage = 1;

                // *** find role id temporarily
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "CUSTOMER")).FirstOrDefault();
                if (role == null)
                {
                    throw new CustomException.InternalServerErrorException("Cannot find role");
                }

                var customer = await _unitOfWork.AccountRepository.GetAsync(a => a.AccountId == customerId && a.RoleId == role.RoleId);
                if(customer == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy khách hàng");
                }
                
                var payments = await _unitOfWork.PaymentHistoryRepository.GetAsync(filter: p => (Status == null || p.Status == Status) && p.AccountId == customerId);

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

                var paymentsWithPagination = await _unitOfWork.PaymentHistoryRepository.GetAsync(filter: p => (Status == null || p.Status == Status) && p.AccountId == customerId, includeProperties: "Account,Booking", pageIndex: PageIndex, pageSize: PageSize);

                foreach (var payment in paymentsWithPagination)
                {
                    var paymentResponse = new PaymentHistoryByCustomerResponse
                    {
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
            catch (Exception ex) { 
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
