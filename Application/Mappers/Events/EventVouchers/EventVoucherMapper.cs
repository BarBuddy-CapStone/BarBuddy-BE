using Application.DTOs.Events.EventVoucher;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.Events.EventVouchers
{
    public class EventVoucherMapper : Profile
    {
        public EventVoucherMapper() { 
            CreateMap<EventVoucherRequest, EventVoucher>().ReverseMap();
            CreateMap<EventVoucher, EventVoucherResponse>().ReverseMap();
        }
    }
}
