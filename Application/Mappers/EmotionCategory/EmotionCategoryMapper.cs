using Application.DTOs.Request.EmotionCategoryRequest;
using Application.DTOs.Response.EmotionCategory;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.EmotionCategory
{
    public class EmotionCategoryMapper : Profile
    {
        public EmotionCategoryMapper()
        {
            CreateMap<EmotionalDrinkCategory, EmotionCategoryResponse>().ReverseMap();
            CreateMap<CreateEmotionCategoryRequest, EmotionalDrinkCategory>().ReverseMap();
        }
    }
}
