using Application.DTOs.PaymentHistory;
using Application.IService;
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
                    var paymentResponse = new PaymentHistoryResponse();

                    paymentResponse.CustomerName = payment.Account.Fullname;
                    paymentResponse.PhoneNumber = payment.Account.Phone;
                    paymentResponse.BarName = (await _unitOfWork.BarRepository.GetByIdAsync(payment.Booking.BarId)).BarName;
                    paymentResponse.TransactionCode = payment.TransactionCode;
                    paymentResponse.Status = payment.Status;
                    paymentResponse.Note = payment.Note;
                    paymentResponse.PaymentDate = payment.PaymentDate;
                    paymentResponse.TotalPrice = payment.TotalPrice;

                    response.Add(paymentResponse);
                }

                return (response, totalPage);
            }
            catch (Exception ex) { 
                throw new Exception(ex.Message);
            }
        }
    }
}
