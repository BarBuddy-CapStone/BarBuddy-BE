using Application.DTOs.BookingTable;
using Application.DTOs.Table;
using Application.DTOs.TableType;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappers.BookingTables
{
    public class FilterBkTableMapper : Profile
    {
        public FilterBkTableMapper()
        {
            CreateMap<Domain.Entities.Table, FilterTableResponse>();
            CreateMap<BookingTable, FilterBkTableResponse>()
                .ForMember(dst => dst.ReservationDate, src => src.MapFrom(x => x.ReservationDate))
                .ForMember(dst => dst.ReservationTime, src => src.MapFrom(x => x.ReservationTime));
            CreateMap<Domain.Entities.TableType, FilterTableTypeReponse>();

            CreateMap<TableHoldInfo, BookingHubResponse>().ReverseMap();
        }
    }
}
