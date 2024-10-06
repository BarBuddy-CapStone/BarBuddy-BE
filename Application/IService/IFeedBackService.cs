using Application.DTOs.Feedback;
using Application.DTOs.Request.FeedBackRequest;
using Application.DTOs.Response.EmotionCategory;
using Application.DTOs.Response.FeedBack;
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
        Task<FeedBackResponse> GetFeedBackByID(Guid id);
        Task<CustomerFeedbackResponse> GetFeedBackByBookingId(Guid BookingId);
        Task<FeedBackResponse> CreateFeedBack(CreateFeedBackRequest request);
        Task<FeedBackResponse> UpdateFeedBack(Guid id, UpdateFeedBackRequest request);
        Task<bool> DeleteUpdateFeedBack(Guid id);
    }
}
