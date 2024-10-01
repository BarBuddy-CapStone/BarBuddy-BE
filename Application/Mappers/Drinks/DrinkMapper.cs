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
            .ForMember(dst => dst.DrinksCategoryName, src => src.MapFrom(x => x.DrinkCategory.DrinksCategoryName))
            .ForMember(dst => dst.BarName, src => src.MapFrom(x => x.Bar.BarName))
            .ForMember(dst => dst.Images, src => src.MapFrom(x => x.Image))
            .ForMember(dst => dst.EmotionsDrink, src => src.MapFrom(x => x.DrinkEmotionalCategories))
            .ReverseMap();

                    CreateMap<DrinkEmotionalCategory, EmotionCategoryResponse>()
            .ForMember(dest => dest.EmotionalDrinksCategoryId, opt => opt.MapFrom(src => src.EmotionalDrinkCategoryId))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.EmotionalDrinkCategory.CategoryName));

            CreateMap<DrinkEmotionalCategory, EmotionCategoryResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.EmotionalDrinkCategory.CategoryName));
        }
    }
}
