using Application.DTOs.Response.EmotionCategory;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class EmotionalDrinkCategory : IEmotionalDrinkCategory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmotionalDrinkCategory (IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmotionCategoryResponse>> GetEmotionCategory()
        {
            var emotionCategory = _unitOfWork.EmotionalDrinkCategoryRepository.Get();
            
            if (!emotionCategory.Any())
            {
                throw new CustomException.DataNotFoundException("No EmotionCategoryDrink in Database");
            }

            var myemotionCategory = _mapper.Map<IEnumerable<EmotionCategoryResponse>>(emotionCategory);

            return myemotionCategory;
        }
    }
}
