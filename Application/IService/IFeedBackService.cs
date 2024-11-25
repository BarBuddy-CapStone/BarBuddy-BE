using Application.DTOs.Feedback;
using Application.DTOs.Request.FeedBackRequest;
using Application.DTOs.Response.EmotionCategory;
using Application.DTOs.Response.FeedBack;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IFeedBackService
    {
        Task<IEnumerable<FeedBackResponse>> GetFeedBack();
        Task<PagingAdminFeedbackResponse> GetFeedBackAdmin(Guid? BarId, bool? Status, ObjectQueryCustom query);
        Task<PagingManagerFeedbackResponse> GetFeedBackManager(Guid BarId, ObjectQueryCustom query);
        Task<FeedBackResponse> GetFeedBackByID(Guid id);
        Task<CustomerFeedbackResponse> GetFeedBackByBookingId(Guid BookingId);
        Task<FeedBackResponse> CreateFeedBack(CreateFeedBackRequest request);
        Task<FeedBackResponse> UpdateFeedBack(Guid id, UpdateFeedBackRequest request);
        Task UpdateFeedBackByAdmin(Guid FeedbackId, bool Status);
        Task<bool> DeleteUpdateFeedBack(Guid id);
    }
}
