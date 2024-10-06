using Application.Common;
using Application.DTOs.Account;
using Application.DTOs.Authen;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Microsoft.IdentityModel.Tokens;
using static Domain.CustomException.CustomException;

namespace Application.Service
{
    public class AuthenService : IAuthenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthentication _authentication;

        public AuthenService(IUnitOfWork unitOfWork, IMapper mapper,
                            IAuthentication authentication)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authentication = authentication;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                var hashPass = await _authentication.HashedPassword(request.Password);
                var isExist = await _unitOfWork.AccountRepository
                                        .GetAsync(filter: x => x.Email.Equals(request.Email)
                                        && x.Password.Equals(hashPass)
                                        && x.Status == (int)PrefixValueEnum.Active, 
                                        includeProperties: "Role");

                var getOne = isExist.FirstOrDefault();

                if(getOne == null)
                {
                    throw new CustomException.InvalidDataException("Sai tài khoản hoặc mật khẩu! Vui lòng kiểm tra lại.");
                }

                var response = _mapper.Map<LoginResponse>(getOne);
                response.AccessToken = _authentication.GenerteDefaultToken(getOne);

                return response;
            }
            catch (CustomException.InternalServerErrorException e)
            {
            throw new CustomException.InternalServerErrorException("Đã xảy ra lỗi");
            }
        }

        public async Task<CustomerAccountResponse> Register(RegisterRequest request)
        {
            try
            {
                var existEmail = _unitOfWork.AccountRepository
                                       .Get(filter: x => x.Email.Equals(request.Email)).FirstOrDefault();
                var role = _unitOfWork.RoleRepository.Get(x => x.RoleName == "CUSTOMER").FirstOrDefault();

                if (role == null)
                {
                    throw new ForbbidenException("Bạn không thể truy cập");
                }

                if (existEmail != null)
                {
                    throw new CustomException.InvalidDataException("Email đã tồn tại! Vui lòng thử lại.");
                }

                if(!request.Password.Equals(request.ConfirmPassword))
                {
                    throw new CustomException.InvalidDataException("Mật khẩu không giống nhau! Vui lòng thử lại.");
                }

                var mapper = _mapper.Map<Account>(request);

                mapper.Password = await _authentication.HashedPassword(request.Password);
                mapper.CreatedAt = DateTimeOffset.Now;
                mapper.UpdatedAt = mapper.CreatedAt;
                mapper.RoleId = role.RoleId;
                mapper.Status = (int)PrefixValueEnum.Active;
                mapper.Image = "https://th.bing.com/th/id/R.c2d58313dad3a99231d27d38bc77e9bb?rik=G%2bdXJz91ZlvDaw&pid=ImgRaw&r=0";

                await _unitOfWork.AccountRepository.InsertAsync(mapper);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();

                var response = _mapper.Map<CustomerAccountResponse>(mapper);

                return response;
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException("asdasda");
            }
        }
    }
}
