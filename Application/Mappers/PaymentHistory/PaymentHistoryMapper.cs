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
        CreateMap<PaymentHistoryRequest, CreatePayment>()
            .ForMember(cp => cp.PaymentDestinationId, opt => opt.MapFrom(phr => phr.ProviderName))
            .ForMember(cp => cp.RequiredAmount, opt => opt.MapFrom(phr => phr.TotalPrice))
            .ForMember(cp => cp.PaymentRefId, opt => opt.MapFrom(phr => phr.TransactionCode))
            .ReverseMap();
        CreateMap<PaymentHistory, PaymentHistoryResponse>()
            .ForMember(ph => ph.CustomerName, opt => opt.MapFrom(ph => ph.Account.Fullname))
            .ForMember(ph => ph.PhoneNumber, opt => opt.MapFrom(ph => ph.Account.Phone))
            .ForMember(ph => ph.BarName, opt => opt.MapFrom(ph => ph.Booking.Bar.BarName));
    }
}