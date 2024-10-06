using Application.DTOs.Account;
using Application.DTOs.Authen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IAuthenService
    {
        Task<LoginResponse> Login(LoginRequest request);
        Task<CustomerAccountResponse> Register(RegisterRequest request);
    }
}
