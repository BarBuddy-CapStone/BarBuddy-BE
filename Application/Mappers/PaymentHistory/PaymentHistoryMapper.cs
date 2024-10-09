using Application.DTOs.Payment;
using Application.DTOs.PaymentHistory;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PaymentHistoryMapper : Profile
{
    public PaymentHistoryMapper()
    {
        CreateMap<PaymentHistoryRequest, PaymentHistory>().ReverseMap();
        CreateMap<PaymentHistory, PaymentDetailResponse>();

        CreateMap<PaymentHistory, PaymentHistoryResponse>()
            .ForMember(ph => ph.CustomerName, opt => opt.MapFrom(ph => ph.Account.Fullname))
            .ForMember(ph => ph.PhoneNumber, opt => opt.MapFrom(ph => ph.Account.Phone))
            .ForMember(ph => ph.BarName, opt => opt.MapFrom(ph => ph.Booking.Bar.BarName));
    }
}