using Application.DTOs.Request.FeedBackRequest;
using Application.DTOs.Response.FeedBack;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;

namespace Application.Service
{
    public class FeedBackService : IFeedBackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthentication _authentication;

        public FeedBackService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IAuthentication authentication)
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
            var userId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);

            var user = _unitOfWork.AccountRepository.Get(u => u.AccountId == userId).FirstOrDefault();

            var feedbackRequest = _mapper.Map<EmotionalDrinkCategory>(request);
            _unitOfWork.EmotionalDrinkCategoryRepository.Insert(feedbackRequest);
            await _unitOfWork.SaveAsync();

            var feedbackResponse = _mapper.Map<FeedBackResponse>(feedbackRequest);
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

    }
}
