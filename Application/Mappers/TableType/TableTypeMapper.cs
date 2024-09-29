using Application.DTOs.TableType;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Mappers.TableType
{
    public class TableTypeMapper : Profile
    {
        public TableTypeMapper()
        {
            CreateMap<Domain.Entities.TableType, TableTypeResponse>().ReverseMap();
            CreateMap<Domain.Entities.TableType, TableTypeRequest>().ReverseMap();
        }
    }
}
