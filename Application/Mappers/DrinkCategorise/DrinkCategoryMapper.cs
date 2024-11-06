using Application.DTOs.DrinkCategory;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappers.DrinkCategorise
{
    public class DrinkCategoryMapper : Profile
    {
        public DrinkCategoryMapper()
        {
            CreateMap<DrinkCategoryRequest, DrinkCategory>().ReverseMap();
            CreateMap<DrinkCategory, DrinkCategoryResponse>().ReverseMap();
            CreateMap<UpdDrinkCategoryRequest, DrinkCategory>().ReverseMap();
            CreateMap<DeleteDrinkCateRequest, DrinkCategory>();
        }
    }
}
