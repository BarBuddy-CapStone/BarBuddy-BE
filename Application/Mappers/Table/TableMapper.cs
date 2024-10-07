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
            CreateMap<Domain.Entities.Table, TableResponse>()
                .ForMember(a => a.TableTypeName, opt => opt.MapFrom(b => b.TableType.TypeName))
                .ForMember(a => a.MaximumGuest, opt => opt.MapFrom(b => b.TableType.MaximumGuest))
                .ForMember(a => a.MinimumGuest, opt => opt.MapFrom(b => b.TableType.MinimumGuest))
                .ForMember(a => a.MinimumPrice, opt => opt.MapFrom(b => b.TableType.MinimumPrice));
        }
    }
}
