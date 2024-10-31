using Application.DTOs.Drink;
using Application.DTOs.Response.EmotionCategory;
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
            .ForMember(dst => dst.DrinkCategoryResponse, src => src.MapFrom(x => x.DrinkCategory))
            .ForMember(dst => dst.Images, src => src.MapFrom(x => x.Image))
            .ForMember(dst => dst.EmotionsDrink, src => src.MapFrom(x => x.DrinkEmotionalCategories))
            .ForMember(dst => dst.BarName, src => src.MapFrom(x => x.DrinkCategory.Bar.BarName))
            .ReverseMap();

            CreateMap<DrinkEmotionalCategory, EmotionCategoryResponse>()
            .ForMember(dest => dest.EmotionalDrinksCategoryId, opt => opt.MapFrom(src => src.EmotionalDrinkCategory.EmotionalDrinksCategoryId))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.EmotionalDrinkCategory.CategoryName))
            .ForMember(dest => dest.BarId, opt => opt.MapFrom(src => src.Drink.DrinkCategory.BarId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.EmotionalDrinkCategory.Description));
        }
    }
}
