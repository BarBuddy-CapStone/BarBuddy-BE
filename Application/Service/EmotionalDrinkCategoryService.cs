using Application.DTOs.Bar;
using Application.DTOs.Request.EmotionCategoryRequest;
using Application.DTOs.Response.EmotionCategory;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class EmotionalDrinkCategoryService : IEmotionalDrinkCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmotionalDrinkCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
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

        public async Task<EmotionCategoryResponse> GetEmotionCategoryByID(Guid id)
        {
            var emotionid = _unitOfWork.EmotionalDrinkCategoryRepository.Get(e => e.EmotionalDrinksCategoryId == id).FirstOrDefault();
            if (emotionid == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy Trạng Thái.");
            }
            var emotionResponse = _mapper.Map<EmotionCategoryResponse>(emotionid);
            return emotionResponse;
        }

        public async Task<EmotionCategoryResponse> CreateEmotionCategory(CreateEmotionCategoryRequest request)
        {
            var emotionRequest = _mapper.Map<EmotionalDrinkCategory>(request);
            _unitOfWork.EmotionalDrinkCategoryRepository.Insert(emotionRequest);
            await _unitOfWork.SaveAsync();

            var emotionResponse = _mapper.Map<EmotionCategoryResponse>(emotionRequest);
            return emotionResponse;
        }

        public async Task<EmotionCategoryResponse> UpdateEmotionCategory(Guid id, UpdateEmotionCategoryRequest request)
        {
            var emotionCategoryID = _unitOfWork.EmotionalDrinkCategoryRepository.Get(e => e.EmotionalDrinksCategoryId == id).FirstOrDefault();

            if (emotionCategoryID == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy Trạng Thái.");
            }

            var emotionCategory = _mapper.Map(request, emotionCategoryID);

            _unitOfWork.EmotionalDrinkCategoryRepository.Update(emotionCategory);
            await _unitOfWork.SaveAsync();

            var emotionCategoryResponse = _mapper.Map<EmotionCategoryResponse>(emotionCategory);
            return emotionCategoryResponse;
        }

        public async Task<EmotionalDrinkCategory> DeleteEmotionCategory(Guid id)
        {
            var emotionid = _unitOfWork.EmotionalDrinkCategoryRepository.Get(e => e.EmotionalDrinksCategoryId == id).FirstOrDefault();
            if (emotionid == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy Trạng Thái.");
            }

            _unitOfWork.EmotionalDrinkCategoryRepository.Delete(emotionid);
            await _unitOfWork.SaveAsync();

            return emotionid;
        }
    }
}
