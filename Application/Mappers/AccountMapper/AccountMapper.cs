﻿using Application.DTOs.Account;
using Application.DTOs.Authen;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.AccountMapper
{
    public class AccountMapper : Profile
    {
        public AccountMapper()
        {
            CreateMap<CustomerAccountRequest, Account>();
            CreateMap<Account, CustomerAccountResponse>();
            CreateMap<ManagerAccountRequest, Account>();
            CreateMap<StaffAccountRequest, Account>();
            CreateMap<Account, StaffAccountResponse>();
            CreateMap<Account, ManagerAccountResponse>();
            CreateMap<Account, CustomerInfoResponse>().ReverseMap();
            CreateMap<CustomerInfoRequest, Account>().ReverseMap();
            CreateMap<Account, LoginResponse>()
                .ForMember(dst => dst.IdentityId, src => src.MapFrom(x => x.BarId))
                .ForMember(dst => dst.BarName, src => src.MapFrom(x => x.Bar.BarName));
            CreateMap<RegisterRequest, Account>();
        }
    }
}
