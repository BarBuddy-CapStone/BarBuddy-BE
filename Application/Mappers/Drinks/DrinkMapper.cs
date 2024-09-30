using Application.DTOs.Drink;
using AutoMapper;
using Domain.Entities;
namespace Application.Mappers.Drinks
{
    public class DrinkMapper : Profile
    {
        public DrinkMapper()
        {
            CreateMap<DrinkRequest, Drink>().ReverseMap();
            CreateMap<Drink, DrinkResponse>()
                .ForMember(dst => dst.DrinksCategoryName, src => src.MapFrom(x => x.DrinkCategory.DrinksCategoryName))
                .ForMember(dst => dst.BarName, src => src.MapFrom(x => x.Bar.BarName))
                .ReverseMap();
        }
    }
}
