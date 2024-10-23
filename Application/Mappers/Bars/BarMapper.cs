using Application.DTOs.Bar;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.Bars
{
    public class BarMapper : Profile
    {
        public BarMapper() {
            CreateMap<Bar, BarResponse>();
            CreateMap<BarRequest, Bar>().ReverseMap();
        }
    }
}
