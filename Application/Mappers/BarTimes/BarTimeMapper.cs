using Application.DTOs.BarTime;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.BarTimes
{
    public class BarTimeMapper : Profile
    {
        public BarTimeMapper() {
            CreateMap<BarTimeRequest, BarTime>().ReverseMap();
            CreateMap<UpdateBarTimeRequest, BarTime>().ReverseMap();
            CreateMap<BarTime, BarTimeResponse>().ReverseMap();
        }
    }
}
