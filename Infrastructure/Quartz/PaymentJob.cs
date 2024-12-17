using Application.IService;
using Domain.Enums;
using Domain.IRepository;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Quartz
{
    public class PaymentJob : IJob
    {
        private readonly IPaymentHistoryService _paymentHistoryService;
        private readonly IBookingService _bookingService;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentJob(IPaymentHistoryService paymentHistoryService, IBookingService bookingService, IUnitOfWork unitOfWork)
        {
            _paymentHistoryService = paymentHistoryService;
            _bookingService = bookingService;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var getAllPayment = _unitOfWork.PaymentHistoryRepository
                                            .Get(x => x.Status == (int)PaymentStatusEnum.Pending, 
                                                    includeProperties: "Booking");
            DateTimeOffset now = DateTimeOffset.Now;
            TimeSpan roundedTimeOfDay = TimeSpan.FromHours(now.TimeOfDay.Hours)
                                               .Add(TimeSpan.FromMinutes(now.TimeOfDay.Minutes))
                                               .Add(TimeSpan.FromSeconds(now.TimeOfDay.Seconds));

            foreach (var payment in getAllPayment) {
                if((payment.Booking.CreateAt.TimeOfDay) <= roundedTimeOfDay)
                {
                    var getBooking = _unitOfWork.BookingRepository.GetByID(payment.BookingId);
                    if(getBooking.Status == (int)PaymentStatusEnum.Pending)
                    {
                        getBooking.Status = (int)PrefixValueEnum.Cancelled;
                        payment.Status = (int)PaymentStatusEnum.Failed;
                        await _unitOfWork.PaymentHistoryRepository.UpdateAsync(payment);
                        await _unitOfWork.SaveAsync();
                    }
                }
            }
        }
    }
}
