using Application.DTOs.Account;
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
            CreateMap<StaffAccountRequest, Account>();
            CreateMap<Account, StaffAccountResponse>();
        }
    }
}
