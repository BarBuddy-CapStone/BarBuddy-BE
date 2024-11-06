using Application.DTOs.DrinkCategory;
using Application.IService;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class DrinkCategoryService : IDrinkCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DrinkCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DrinkCategoryResponse> CreateDrinkCategory(DrinkCategoryRequest request)
        {
            try
            {
                var isExistName = _unitOfWork.DrinkCategoryRepository
                                                    .Get(filter: x => x.DrinksCategoryName
                                                                    .Contains(request.DrinksCategoryName))
                                                    .FirstOrDefault();

                if (isExistName != null) {
                    throw new CustomException.InvalidDataException("Tên thể loại đồ uống đã tồn tại, vui lòng thử lại");
                }

                var mapper = _mapper.Map<DrinkCategory>(request);
                mapper.IsDrinkCategory = PrefixKeyConstant.TRUE;

                await _unitOfWork.DrinkCategoryRepository.InsertAsync(mapper);
                await Task.Delay(20);
                await _unitOfWork.SaveAsync();

                var response = _mapper.Map<DrinkCategoryResponse>(mapper);

                return response;

            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
            catch (CustomException.DataNotFoundException e)
            {
                throw new CustomException.DataNotFoundException(e.Message);
            }
        }

        public async Task<bool> DeleteDrinkCategory(Guid drinkCateId)
        {
            try
            {
                var getDrinkCateById = await _unitOfWork.DrinkCategoryRepository
                                                        .GetAsync(filter: x => x.DrinksCategoryId.Equals(drinkCateId)
                                                                            && x.IsDrinkCategory == PrefixKeyConstant.TRUE);
                var getOne = getDrinkCateById.FirstOrDefault();

                if (getOne == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy thể loại đồ uống!");
                }

                getOne.IsDrinkCategory = PrefixKeyConstant.FALSE;
                await _unitOfWork.DrinkCategoryRepository.UpdateAsync(getOne);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();

                return true;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
            catch (CustomException.DataNotFoundException e)
            {
                throw new CustomException.DataNotFoundException(e.Message);
            }
        }

        public async Task<IEnumerable<DrinkCategoryResponse>> GetAllDrinkCategory()
        {
            try
            {
                var getAllDrinkCate = await _unitOfWork.DrinkCategoryRepository.GetAsync(filter: x => x.IsDrinkCategory == PrefixKeyConstant.TRUE);

                if (getAllDrinkCate.IsNullOrEmpty())
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy thể loại đồ uống nào !");
                }

                var response = _mapper.Map<IEnumerable<DrinkCategoryResponse>>(getAllDrinkCate);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
            catch (CustomException.DataNotFoundException e)
            {
                throw new CustomException.DataNotFoundException(e.Message);
            }
        }

        public async Task<IEnumerable<DrinkCategoryResponse>> GetAllDrinkCateOfBar(Guid barId)
        {
            try
            {
                var getAllDrinkCateOfBar = await _unitOfWork.DrinkCategoryRepository
                                                            .GetAsync(filter: x => /*x.BarId.Equals(barId) 
                                                            &&*/ x.IsDrinkCategory == PrefixKeyConstant.TRUE);

                if (getAllDrinkCateOfBar.IsNullOrEmpty())
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy danh sách thể loại đồ uống nào của quán bar !");
                }

                var response = _mapper.Map<IEnumerable<DrinkCategoryResponse>>(getAllDrinkCateOfBar);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
            catch (CustomException.DataNotFoundException e)
            {
                throw new CustomException.DataNotFoundException(e.Message);
            }
        }

        public async Task<DrinkCategoryResponse> GetDrinkCategoryById(Guid drinkCateId)
        {
            try
            {
                var getDrinkCateById = await _unitOfWork.DrinkCategoryRepository
                                        .GetAsync(filter: x => x.DrinksCategoryId.Equals(drinkCateId)
                                                            && x.IsDrinkCategory == PrefixKeyConstant.TRUE);
                var getOne = getDrinkCateById.FirstOrDefault();

                if (getOne == null)
                {
                    throw new CustomException.DataNotFoundException("The category drink is empty !");
                }

                var response = _mapper.Map<DrinkCategoryResponse>(getOne);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
        }

        public async Task<DrinkCategoryResponse> UpdateDrinkCategory(Guid drinkCateId, DrinkCategoryRequest request)
        {
            try
            {


                var getDrinkCateById = await _unitOfWork.DrinkCategoryRepository
                                        .GetAsync(filter: x => x.DrinksCategoryId.Equals(drinkCateId)
                                                            && x.IsDrinkCategory == PrefixKeyConstant.TRUE);
                var getOne = getDrinkCateById.FirstOrDefault();

                if (getOne == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy thể loại đồ uống !");
                }

                var mapper = _mapper.Map(request, getOne);
                await _unitOfWork.DrinkCategoryRepository.UpdateAsync(mapper);
                await Task.Delay(20);
                await _unitOfWork.SaveAsync();

                var response = _mapper.Map<DrinkCategoryResponse>(mapper);
                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
                throw new CustomException.InternalServerErrorException(e.Message);
            }
            catch (CustomException.DataNotFoundException e)
            {
                throw new CustomException.DataNotFoundException(e.Message);
            }
        }
    }
}
    