using Application.DTOs.Feedback;
using Application.DTOs.Request.FeedBackRequest;
using Application.DTOs.Response.FeedBack;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.FeedBack
{
    public class FeedBackMapper : Profile
    {
        public FeedBackMapper()
        {
            CreateMap<Feedback, FeedBackResponse>().ReverseMap();
            CreateMap<CreateFeedBackRequest, Feedback>().ReverseMap();
            CreateMap<UpdateFeedBackRequest, Feedback>().ReverseMap();
            CreateMap<DeleteFeedBackRequest, Feedback>().ReverseMap();
            CreateMap<Feedback, AdminFeedbackResponse>()
                .ForMember(x => x.CustomerName, opt => opt.MapFrom(x => x.Account.Fullname))
                .ForMember(x => x.BarName, opt => opt.MapFrom(x => x.Bar.BarName));
        }
    }
}
