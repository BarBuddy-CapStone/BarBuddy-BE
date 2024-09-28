using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.TableTypeDto;

namespace Application.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() {
            CreateMap<TableType, TableTypeDtoResponse>().ReverseMap();
            CreateMap<TableType, TableTypeDtoRequest>().ReverseMap();
        }
    }
}
