using Application.DTOs.Table;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.Table
{
    public class TableMapper : Profile
    {
        public TableMapper()
        {
            CreateMap<Domain.Entities.Table, TableResponse>();
        }
    }
}
