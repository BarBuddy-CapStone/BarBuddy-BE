using Application.DTOs.Bar;
using Application.DTOs.Request.EmotionCategoryRequest;
using Application.DTOs.Response.EmotionCategory;
using Application.IService;
using AutoMapper;
using Azure.Core;
using Domain.Common;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task<IEnumerable<EmotionCategoryResponse>> GetEmotionCategory(ObjectQuery query)
        {
            Expression<Func<EmotionalDrinkCategory, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                filter = bar => bar.CategoryName.Contains(query.Search);
            }

            var emotionCategory = _unitOfWork.EmotionalDrinkCategoryRepository
                                                .Get(filter: filter,
                                                    pageIndex: query.PageIndex,
                                                    pageSize: query.PageSize)
                                                .Where(e => e.IsDeleted == PrefixKeyConstant.FALSE);

            if (!emotionCategory.Any())
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy danh mục cảm xúc !");
            }

            var myemotionCategory = _mapper.Map<IEnumerable<EmotionCategoryResponse>>(emotionCategory);

            return myemotionCategory;
        }

        public async Task<EmotionCategoryResponse> GetEmotionCategoryByID(Guid id)
        {
            var emotionid = _unitOfWork.EmotionalDrinkCategoryRepository
                                        .Get(e => e.EmotionalDrinksCategoryId.Equals(id) 
                                            && e.IsDeleted == PrefixKeyConstant.FALSE)
                                        .FirstOrDefault();
            if (emotionid == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy thể loại cảm xúc, vui lòng thử lại !");
            }
            var emotionResponse = _mapper.Map<EmotionCategoryResponse>(emotionid);
            return emotionResponse;
        }

        public async Task<List<EmotionCategoryResponse>> GetEmotionCategoryOfBar(Guid barId)
        {
            var emotionid = await _unitOfWork.EmotionalDrinkCategoryRepository
                                        .GetAsync(e => /*e.BarId.Equals(barId)
                                            &&*/ e.IsDeleted == PrefixKeyConstant.FALSE);
            if (emotionid == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy thể loại cảm xúc, vui lòng thử lại !");
            }

            var isExistBar = _unitOfWork.BarRepository.Exists(filter: x => x.BarId.Equals(barId));

            if (!isExistBar)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy quán bar, vui lòng thử lại!");
            }

            var emotionResponse = _mapper.Map<List<EmotionCategoryResponse>>(emotionid);
            return emotionResponse;
        }

        public async Task<EmotionCategoryResponse> CreateEmotionCategory(CreateEmotionCategoryRequest request)
        {
            var isExistName = _unitOfWork.EmotionalDrinkCategoryRepository
                                            .Exists(filter: x => x.CategoryName.Contains(request.CategoryName));
            
            if (isExistName) {
                throw new CustomException.InvalidDataException("Tên thể loại cảm xúc đã tồn tại, vui lòng thử lại!");
            }

            var emotionRequest = _mapper.Map<EmotionalDrinkCategory>(request);
            emotionRequest.IsDeleted = PrefixKeyConstant.FALSE;
            _unitOfWork.EmotionalDrinkCategoryRepository.Insert(emotionRequest);
            await _unitOfWork.SaveAsync();

            var emotionResponse = _mapper.Map<EmotionCategoryResponse>(emotionRequest);
            return emotionResponse;
        }

        public async Task<EmotionCategoryResponse> UpdateEmotionCategory(Guid id, UpdateEmotionCategoryRequest request)
        {
            var emotionCategoryID = _unitOfWork.EmotionalDrinkCategoryRepository
                                                .Get(e => e.EmotionalDrinksCategoryId.Equals(id) 
                                                        && e.IsDeleted == PrefixKeyConstant.FALSE)
                                                .FirstOrDefault();
            if (emotionCategoryID == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy thể loại cảm xúc, vui lòng thử lại !");
            }

            var emotionCategory = _mapper.Map(request, emotionCategoryID);
            emotionCategory.IsDeleted = PrefixKeyConstant.FALSE;

            _unitOfWork.EmotionalDrinkCategoryRepository.Update(emotionCategory);
            await _unitOfWork.SaveAsync();

            var emotionCategoryResponse = _mapper.Map<EmotionCategoryResponse>(emotionCategory);
            return emotionCategoryResponse;
        }

        public async Task DeleteEmotionCategory(Guid id)
        {
            var emotionid = _unitOfWork.EmotionalDrinkCategoryRepository
                                        .Get(e => e.EmotionalDrinksCategoryId.Equals(id) 
                                                && e.IsDeleted == PrefixKeyConstant.FALSE)
                                        .FirstOrDefault();
            if (emotionid == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy thể loại cảm xúc, vui lòng thử lại !");
            }

            emotionid.IsDeleted = PrefixKeyConstant.TRUE;
            await _unitOfWork.EmotionalDrinkCategoryRepository.UpdateRangeAsync(emotionid);
            await Task.Delay(10);
            await _unitOfWork.SaveAsync();
        }
    }
}
