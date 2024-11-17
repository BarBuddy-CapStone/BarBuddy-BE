using Application.Common;
using Application.DTOs.Account;
using Application.DTOs.Authen;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Azure;
using Azure.Core;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Domain.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using static Domain.CustomException.CustomException;

namespace Application.Service
{
    public class AuthenService : IAuthenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthentication _authentication;
        private readonly IMemoryCache _cache;
        private readonly IOtpSender _otpSender;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IFcmService _fcmService;

        public AuthenService(IUnitOfWork unitOfWork, IMapper mapper,
                            IAuthentication authentication, IMemoryCache cache,
                            IOtpSender otpSender, IGoogleAuthService googleAuthService,
                            IEmailSender emailSender, IConfiguration configuration,
                            IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authentication = authentication;
            _cache = cache;
            _otpSender = otpSender;
            _googleAuthService = googleAuthService;
            _emailSender = emailSender;
            _configuration = configuration;
            _fcmService = fcmService;
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

                if (!string.IsNullOrEmpty(request.DeviceToken))
                {
                    await _fcmService.SaveUserDeviceToken(
                        getOne.AccountId,
                        request.DeviceToken,
                        request.Platform ?? "unknown");
                }

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
                throw new CustomException.InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<bool> RegisterWithOtp(RegisterRequest request)
        {
            bool flag = false;
            var existEmail = _unitOfWork.AccountRepository
                                   .Get(filter: x => x.Email.Equals(request.Email)).FirstOrDefault();
            var role = _unitOfWork.RoleRepository.Get(x => x.RoleName == "CUSTOMER").FirstOrDefault();

            if (role == null)
            {
                throw new ForbbidenException("Bạn không thể truy cập");
            }

            if (existEmail != null && existEmail.Status == (int)PrefixValueEnum.Pending)
            {
                _cache.Remove(existEmail);
                await _otpSender.SendOtpAsync(existEmail.Email);
                return flag = true;
            }
            else if (existEmail != null && existEmail.Status == (int)PrefixValueEnum.Active)
            {
                throw new DataExistException("Email đã tồn tại");
            }

            if (!request.Password.Equals(request.ConfirmPassword))
            {
                throw new CustomException.InvalidDataException("Mật khẩu không giống nhau! Vui lòng thử lại.");
            }

            try
            {
                var mapper = _mapper.Map<Account>(request);

                mapper.Password = await _authentication.HashedPassword(request.Password);
                mapper.CreatedAt = DateTimeOffset.Now;
                mapper.UpdatedAt = mapper.CreatedAt;
                mapper.RoleId = role.RoleId;
                mapper.Status = (int)PrefixValueEnum.Pending;
                mapper.Image = "https://th.bing.com/th/id/R.c2d58313dad3a99231d27d38bc77e9bb?rik=G%2bdXJz91ZlvDaw&pid=ImgRaw&r=0";

                _unitOfWork.BeginTransaction();
                await _unitOfWork.AccountRepository.InsertAsync(mapper);
                _unitOfWork.CommitTransaction();
                await _unitOfWork.SaveAsync();
                await _otpSender.SendOtpAsync(mapper.Email);
                flag = true;

            } catch(Exception ex)
            {
                _unitOfWork.RollBack();
                throw new Exception($"Lỗi hệ thống: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }

            return flag;
        }

        public async Task<bool> ConfirmAccountByOtp(OtpVerificationRequest request)
        {
            bool flag = false;
            var existAccount = _unitOfWork.AccountRepository
                .Get(filter: x => x.Email.Equals(request.Email)).FirstOrDefault();
            if (existAccount == null)
            {
                throw new DataNotFoundException("Tài khoản không tồn tại");
            } else if (existAccount.Status == (int)PrefixValueEnum.Active)
            {
                throw new ForbbidenException("Tài khoản đã được kích hoạt");
            }

            var validOtp = _cache.Get(request.Email)?.ToString();
            if (validOtp.IsNullOrEmpty() || validOtp != request.Otp)
            {
                return flag;
            }

            try
            {
                _unitOfWork.BeginTransaction();
                existAccount.Status = (int)PrefixValueEnum.Active;
                _unitOfWork.AccountRepository.Update(existAccount);
                _unitOfWork.CommitTransaction();
                await _unitOfWork.SaveAsync();
                flag = true;
            } catch (Exception ex)
            {
                _unitOfWork.RollBack();
            } finally
            {
                _unitOfWork.Dispose();
            }
            return flag;
        }

        public async Task<bool> ResetPassword(string email)
        {
            bool flag = false;
            var existAccount = _unitOfWork.AccountRepository
                .Get(filter: x => x.Email.Equals(email)).FirstOrDefault();
            if (existAccount == null)
            {
                throw new DataNotFoundException("Tài khoản không tồn tại");
            }
            else if (existAccount.Status == (int)PrefixValueEnum.Inactive)
            {
                throw new ForbbidenException("Tài khoản ngưng hoạt động cần liên hệ admin");
            }

            try
            {
                var randomPassword = RandomHelper.GenerateRandomString();
                _unitOfWork.BeginTransaction();
                existAccount.Password = await _authentication.HashedPassword(randomPassword);
                _unitOfWork.AccountRepository.Update(existAccount);
                _unitOfWork.CommitTransaction();
                await _unitOfWork.SaveAsync();
                await _emailSender.SendEmail(existAccount.Email, "Reset Password", $"Your new password is {randomPassword}");
                flag = true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
            }
            finally
            {
                _unitOfWork.Dispose();
            }
            return flag;
        }

        public async Task<LoginResponse> GoogleLogin(string idToken)
        {
            try
            {
                var googleUser = await _googleAuthService.AuthenticateGoogleUserAsync(idToken);

                var isExist = (await _unitOfWork.AccountRepository
                                        .GetAsync(filter: x => x.Email.Equals(googleUser.Email)
                                        && x.Status == (int)PrefixValueEnum.Active,
                                        includeProperties: "Role")).FirstOrDefault();

                if (isExist == null)
                {
                    var Role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "CUSTOMER")).FirstOrDefault();

                    var profilePictureUrl = await _googleAuthService.UploadProfilePictureAsync(googleUser.PictureUrl, googleUser.Email);

                    var newCustomer = new Account
                    {
                        Fullname = googleUser.Name,
                        Email = googleUser.Email,
                        Image = profilePictureUrl,
                        CreatedAt = DateTime.Now,
                        Phone = "",
                        Dob = DateTimeOffset.Now,
                        Password = "HowWouldTheyKnow",
                        RoleId = Role == null ? throw new CustomException.DataNotFoundException("Không tìm thấy vai trò") : Role.RoleId,
                        Status = (int)PrefixValueEnum.Active,
                        UpdatedAt = DateTimeOffset.Now,
                    };

                    await _unitOfWork.AccountRepository.InsertAsync(newCustomer);
                    await _unitOfWork.SaveAsync();

                    // Tạo và trả về token JWT
                    var response = _mapper.Map<LoginResponse>(newCustomer);
                    response.AccessToken = _authentication.GenerteDefaultToken(newCustomer);

                    return response;
                }
                else
                {
                    // Người dùng đã tồn tại, cập nhật thông tin nếu cần
                    isExist.Fullname = googleUser.Name;
                    isExist.Image = await _googleAuthService.UploadProfilePictureAsync(googleUser.PictureUrl, googleUser.Email);
                    await _unitOfWork.AccountRepository.UpdateAsync(isExist);

                    // Tạo và trả về token JWT
                    var response = _mapper.Map<LoginResponse>(isExist);
                    response.AccessToken = _authentication.GenerteDefaultToken(isExist);

                    return response;
                }
            }
            catch (Exception ex) { 
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
