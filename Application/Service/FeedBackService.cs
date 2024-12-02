using Application.DTOs.Bar;
using Application.DTOs.Feedback;
using Application.DTOs.Request.FeedBackRequest;
using Application.DTOs.Response.FeedBack;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.Common;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using static Domain.CustomException.CustomException;

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
            var feedback = _unitOfWork.FeedbackRepository.Get(includeProperties: "Account,Bar");

            if (!feedback.Any())
            {
                throw new CustomException.DataNotFoundException("No feedback in Database");
            }

            var myfeedback = _mapper.Map<IEnumerable<FeedBackResponse>>(feedback);

            return myfeedback;
        }

        public async Task<FeedBackResponse> GetFeedBackByID(Guid id)
        {
            var feedbackId = _unitOfWork.FeedbackRepository.Get(fb => fb.FeedbackId == id, includeProperties: "Account,Bar").FirstOrDefault();
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

            var booking = (await _unitOfWork.BookingRepository
                                            .GetAsync(b => b.BookingId == request.BookingId &&
                                                           b.AccountId.Equals(userId)))
                                            .FirstOrDefault();

            if (booking == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy booking");
            } else if(booking.Status != 3)
            {
                throw new CustomException.InvalidDataException("Không thể đánh giá, lịch đặt bàn chưa hoàn thành");
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
                var accountId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var feedback = (await _unitOfWork.FeedbackRepository
                                                 .GetAsync(f => f.BookingId == BookingId && 
                                                                f.Booking.AccountId.Equals(accountId), 
                                                                includeProperties: "Account,Bar,Booking"))
                                                 .FirstOrDefault();
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
                    //EndTime = feedback.Bar.EndTime,
                    //StartTime = feedback.Bar.StartTime
                };
                return response;
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex) {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }


        public async Task<PagingAdminFeedbackResponse> GetFeedBackAdmin(Guid? BarId, bool? Status, ObjectQueryCustom query)
        {
            try
            {
                Expression<Func<Feedback, bool>> filter = null;
                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    filter = feedback => feedback.Account != null &&
                                       feedback.Account.Fullname != null &&
                                       feedback.Account.Fullname.Contains(query.Search);
                }

                var feedbacks = await _unitOfWork.FeedbackRepository
                                .GetAsync(filter: f => (BarId == null || f.BarId == BarId) &&
                                                     (Status == null || f.IsDeleted == Status) &&
                                                     (string.IsNullOrEmpty(query.Search) ||
                                                      (f.Account != null &&
                                                       f.Account.Fullname != null &&
                                                       f.Account.Fullname.Contains(query.Search))),
                                        includeProperties: "Account,Bar",
                                        orderBy: o => o.OrderByDescending(f => f.CreatedTime)
                                                     .ThenByDescending(f => f.LastUpdatedTime));

                if (!feedbacks.Any())
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy phản hồi nào!");
                }

                var response = _mapper.Map<List<AdminFeedbackResponse>>(feedbacks);

                var pageIndex = query.PageIndex ?? 1;
                var pageSize = query.PageSize ?? 6;

                var totalItems = response.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var paginatedFeedbacks = response.Skip((pageIndex - 1) * pageSize)
                                               .Take(pageSize)
                                               .ToList();

                return new PagingAdminFeedbackResponse
                {
                    AdminFeedbackResponses = paginatedFeedbacks,
                    TotalPages = totalPages,
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task UpdateFeedBackByAdmin(Guid FeedbackId, bool Status)
        {
            try
            {
                var feedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(FeedbackId);
                if (feedback == null)
                {
                    throw new Exception("Không thể tìm thấy đánh giá");
                }
                feedback.IsDeleted = Status;
                await _unitOfWork.FeedbackRepository.UpdateAsync(feedback);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex) { 
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<PagingManagerFeedbackResponse> GetFeedBackManager(Guid BarId, ObjectQueryCustom query)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                if (!getAccount.BarId.Equals(BarId))
                {
                    throw new UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                var pageIndex = query.PageIndex ?? 1;
                var pageSize = query.PageSize ?? 6;
                var feedbacks = await _unitOfWork.FeedbackRepository.GetAsync(
                   f => f.BarId.Equals(BarId) && f.IsDeleted == false);
                if (!feedbacks.Any())
                {
                    throw new DataNotFoundException("Không tìm thấy phản hồi nào!");
                }
                var totalItems = feedbacks.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var feedbacksWithPagination = await _unitOfWork.FeedbackRepository.GetAsync(
                   f => f.BarId.Equals(BarId) && f.IsDeleted == false &&
                       (string.IsNullOrEmpty(query.Search) ||
                       (f.Account != null &&
                        f.Account.Fullname != null &&
                        f.Account.Fullname.Contains(query.Search))),
                   pageIndex: pageIndex,
                   pageSize: pageSize,
                   includeProperties: "Account,Bar",
                   orderBy: o => o.OrderByDescending(f => f.CreatedTime)
                                 .ThenByDescending(f => f.LastUpdatedTime));
                var responses = feedbacksWithPagination.Select(feedback =>
                   _mapper.Map<ManagerFeedbackResponse>(feedback)).ToList();
                return new PagingManagerFeedbackResponse
                {
                    ManagerFeedbackResponses = responses,
                    TotalPages = totalPages,
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                throw new CustomException.UnAuthorizedException(ex.Message);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
