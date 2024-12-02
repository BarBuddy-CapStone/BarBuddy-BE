using Application.DTOs.Booking;
using Application.DTOs.BookingDrink;
using Application.DTOs.BookingExtraDrink;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.BookingExtraDrinkMapper
{
    public class BkExtraDrinkMapper : Profile
    {
        public BkExtraDrinkMapper()
        {
            CreateMap<UpdBkDrinkExtraRequest, BookingExtraDrink>();
            CreateMap<DrinkRequest, BookingExtraDrink>();
            CreateMap<BookingExtraDrink, BookingDrinkDetailResponse>()
                .ForMember(dst => dst.DrinkName, src => src.MapFrom(x => x.Drink.DrinkName))
                .ForMember(dst => dst.Image, src => src.MapFrom(x => x.Drink.Image));
            CreateMap<BookingExtraDrink, BookingDrinkExtraResponse>()
                .ForMember(dst => dst.DrinkName, src => src.MapFrom(x => x.Drink.DrinkName))
                .ForMember(dst => dst.Image, src => src.MapFrom(x => x.Drink.Image));
        }
    }
}
