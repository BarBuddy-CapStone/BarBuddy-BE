using Application.DTOs.Tokens;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.Tokens
{
    public class TokenMapper : Profile
    {
        public TokenMapper() { 
            CreateMap<Token, TokenResponse>();
        }
    }
}
