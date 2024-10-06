using Application.DTOs.Bar;
using Application.DTOs.Feedback;
using Application.DTOs.Request.FeedBackRequest;
using Application.DTOs.Response.EmotionCategory;
using Application.DTOs.Response.FeedBack;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.Interfaces;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class FeedBackService : IFeedBackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthentication _authentication;

        public FeedBackService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor ,IAuthentication authentication)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _authentication = authentication;
        }

        public async Task<IEnumerable<FeedBackResponse>> GetFeedBack()
        {
            var feedback = _unitOfWork.FeedbackRepository.Get();

            if (!feedback.Any())
            {
                throw new CustomException.DataNotFoundException("No feedback in Database");
            }

            var myfeedback = _mapper.Map<IEnumerable<FeedBackResponse>>(feedback);

            return myfeedback;
        }

        public async Task<FeedBackResponse> GetFeedBackByID(Guid id)
        {
            var feedbackId = _unitOfWork.FeedbackRepository.Get(fb => fb.FeedbackId == id).FirstOrDefault();
            if (feedbackId == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy Feedback.");
            }
            var feedbackResponse = _mapper.Map<FeedBackResponse>(feedbackId);
            return feedbackResponse;
        }

        public async Task<FeedBackResponse> CreateFeedBack(CreateFeedBackRequest request)
        {
            //var userId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);

            //var user = _unitOfWork.AccountRepository.Get(u => u.AccountId == userId).FirstOrDefault();

            //var feedbackRequest = _mapper.Map<EmotionalDrinkCategory>(request);
            //_unitOfWork.EmotionalDrinkCategoryRepository.Insert(feedbackRequest);
            //await _unitOfWork.SaveAsync();

            var booking = (await _unitOfWork.BookingRepository.GetAsync(b => b.BookingId == request.BookingId)).FirstOrDefault();

            if (booking == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy booking");
            }

            var createTime = DateTimeOffset.Now;

            var feedback = new Feedback
            {
                BarId = booking.BarId,
                AccountId = booking.AccountId,
                BookingId = booking.BookingId,
                Comment = request.Comment,
                CreatedTime = createTime,
                IsDeleted = false,
                LastUpdatedTime = createTime,
                Rating = request.Rating
            };

            await _unitOfWork.FeedbackRepository.InsertAsync(feedback);
            await _unitOfWork.SaveAsync();

            var feedbackResponse = _mapper.Map<FeedBackResponse>(feedback);
            return feedbackResponse;
        }

        public async Task<FeedBackResponse> UpdateFeedBack(Guid id, UpdateFeedBackRequest request)
        {
            //var userId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);

            //var user = _unitOfWork.UserRepository.Get(u => u.Id == userId).FirstOrDefault();

            var feedbackID = _unitOfWork.FeedbackRepository.GetByID(id);

            if (feedbackID == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy Feedback.");
            }

            var feedback = _mapper.Map(request, feedbackID);

            _unitOfWork.FeedbackRepository.Update(feedback);
            await _unitOfWork.SaveAsync();

            var feedbackResponse = _mapper.Map<FeedBackResponse>(feedback);
            return feedbackResponse;
        }

        public async Task<bool> DeleteUpdateFeedBack(Guid id)
        {
            //var userId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);

            //var user = _unitOfWork.UserRepository.Get(u => u.Id == userId).FirstOrDefault();

            var feedbackID = _unitOfWork.FeedbackRepository.GetByID(id);
            if (feedbackID == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy.");
            }

            feedbackID.IsDeleted = true;
            _unitOfWork.FeedbackRepository.Update(feedbackID);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<CustomerFeedbackResponse> GetFeedBackByBookingId(Guid BookingId)
        {
            try
            {
                var feedback = (await _unitOfWork.FeedbackRepository.GetAsync(f => f.BookingId == BookingId, includeProperties: "Account,Bar")).FirstOrDefault();
                if (feedback == null)
                {
                    throw new CustomException.DataNotFoundException("Đã có lỗi xảy ra, đánh giá không tồn tại");
                }
                var response = new CustomerFeedbackResponse
                {
                    BarAddress = feedback.Bar.Address,
                    BarImage = feedback.Bar.Images.Split(',')[0],
                    BarName = feedback.Bar.BarName,
                    Comment = feedback.Comment,
                    CreatedTime = feedback.CreatedTime,
                    CustomerAvatar = feedback.Account.Image,
                    CustomerName = feedback.Account.Fullname,
                    FeedbackId = feedback.FeedbackId,
                    LastUpdatedTime = feedback.LastUpdatedTime,
                    Rating = feedback.Rating,
                    EndTime = feedback.Bar.EndTime,
                    StartTime = feedback.Bar.StartTime
                };
                return response;
            }
            catch (Exception ex) {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
