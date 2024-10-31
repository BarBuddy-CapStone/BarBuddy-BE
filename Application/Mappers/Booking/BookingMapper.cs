using Application.DTOs.Booking;
using Application.DTOs.BookingDrink;
using Application.DTOs.BookingTable;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.Booking
{
    public class BookingMapper : Profile
    {
        public BookingMapper()
        {
            CreateMap<Domain.Entities.Booking, BookingResponse>()
                .ForMember(x => x.BarName, opt => opt.MapFrom(y => y.Bar.BarName))
                .ForMember(x => x.BarAddress, opt => opt.MapFrom(y => y.Bar.Address))
                .ForMember(x => x.CustomerName, opt => opt.MapFrom(y => y.Account.Fullname))
                .ForMember(x => x.CustomerPhone, opt => opt.MapFrom(y => y.Account.Phone))
                .ForMember(x => x.CustomerEmail, opt => opt.MapFrom(y => y.Account.Email));

            CreateMap<BookingTable, BookingTableResponse>();

            CreateMap<BookingTableRequest, Domain.Entities.Booking>();

            CreateMap<BookingDrink, BookingDrinkDetailResponse>()
                .ForMember(x => x.DrinkName, opt => opt.MapFrom(y => y.Drink.DrinkName));

            CreateMap<Domain.Entities.Booking, BookingCustomResponse>()
                .ForMember(dst => dst.BarName, src => src.MapFrom(x => x.Bar.BarName));
        }
    }
}
