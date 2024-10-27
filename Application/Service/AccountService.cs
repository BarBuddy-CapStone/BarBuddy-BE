using Application.Common;
using Application.DTOs.Account;
using Application.DTOs.Bar;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Azure;
using Azure.Core;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;
using static Domain.CustomException.CustomException;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;

namespace Application.Service
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly IGenericRepository<Bar> _barRepository;
        private readonly IFirebase _fireBase;
        private static Random random;
        private static readonly string key = "1234567890123456";

        public AccountService(IMapper mapper, IUnitOfWork unitOfWork, IFirebase fireBase)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _accountRepository = _unitOfWork.AccountRepository;
            _barRepository = _unitOfWork.BarRepository;
            _fireBase = fireBase;
            random = new Random();
        }

        public async Task<CustomerAccountResponse> CreateCustomerAccount(CustomerAccountRequest request)
        {
            var existedAccount = (await _accountRepository.GetAsync(a => a.Email.Equals(request.Email)))
                .FirstOrDefault();
            if (existedAccount != null)
            {
                throw new DataExistException("Email is existed");
            }

            var customerRole = (await _unitOfWork.RoleRepository.GetAsync(filter: r => r.RoleName == "CUSTOMER")).FirstOrDefault();
            
            var newAccount = _mapper.Map<Account>(request);
            newAccount.RoleId = customerRole.RoleId;
            newAccount.Status = 1;
            newAccount.CreatedAt = DateTime.Now;
            newAccount.Password = await HashPassword(RandomString(10));

            try
            {
                _unitOfWork.BeginTransaction();
                _accountRepository.Insert(newAccount);
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InternalServerErrorException($"An Internal error occurred while creating customer: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }

            return _mapper.Map<CustomerAccountResponse>(newAccount);
        }


        public async Task<StaffAccountResponse> CreateStaffAccount(StaffAccountRequest request)
        {
            var existedAccount = (await _accountRepository.GetAsync(a => a.Email.Equals(request.Email))).FirstOrDefault();
            if (existedAccount != null)
            {
                throw new DataExistException("Email is existed");
            }
            var staffRole = (await _unitOfWork.RoleRepository.GetAsync(filter: r => r.RoleName == "STAFF")).FirstOrDefault();
            var existedBar = await _barRepository.GetByIdAsync(request.BarId);
            if (existedBar == null)
            {
                throw new DataNotFoundException("Bar is not found in database");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var newAccount = _mapper.Map<Account>(request);
                newAccount.RoleId = staffRole.RoleId;
                newAccount.Status = 1;
                newAccount.CreatedAt = DateTime.Now;
                newAccount.Password = await HashPassword(RandomString(10));
                _accountRepository.Insert(newAccount);
                _unitOfWork.CommitTransaction();
                _unitOfWork.Save();
                var result = _mapper.Map<StaffAccountResponse>(newAccount);
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InternalServerErrorException($"An Internal error occurred while creating staff: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }
        private async Task<string> HashPassword(string password)
        {
            try
            {
                using (SHA512 sha512 = SHA512.Create())
                {
                    byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));

                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        stringBuilder.Append(hashBytes[i].ToString("x2"));
                    }

                    return await Task.FromResult(stringBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<StaffAccountResponse> UpdateStaffAccount(Guid accountId, StaffAccountRequest request)
        {
            var existedAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == accountId && a.Role.RoleName == "STAFF"))
                .FirstOrDefault();
            var existedBar = await _barRepository.GetByIdAsync(request.BarId);
            if (existedAccount == null)
            {
                throw new DataNotFoundException("Staff's account is not found in database");
            }
            if (existedBar == null)
            {
                throw new DataNotFoundException("Bar is not found in database");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var updatedAccount = _mapper.Map(request, existedAccount);
                updatedAccount.UpdatedAt = DateTime.Now;
                _accountRepository.Update(updatedAccount);
                _unitOfWork.CommitTransaction();
                _unitOfWork.Save();
                var result = _mapper.Map<StaffAccountResponse>(updatedAccount);
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InternalServerErrorException($"An Internal error occurred while creating staff: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<CustomerAccountResponse> UpdateCustomerAccount(Guid accountId, CustomerAccountRequest request)
        {
            var existedAccount = (await _accountRepository
                    .GetAsync(filter: a => a.AccountId == accountId && a.Role.RoleName == "CUSTOMER"))
                .FirstOrDefault();
            if (existedAccount == null)
            {
                throw new DataNotFoundException("Customer's account is not found in database");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var updatedAccount = _mapper.Map(request, existedAccount);
                updatedAccount.UpdatedAt = DateTime.Now;
                await _accountRepository.UpdateAsync(updatedAccount);
                _unitOfWork.CommitTransaction();
                _unitOfWork.Save();
                var result = _mapper.Map<CustomerAccountResponse>(updatedAccount);
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InternalServerErrorException($"An Internal error occurred while creating staff: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<bool> BlockOrUnblockAccount(string accountId, bool isBlock)
        {
            var existedAccount = _accountRepository.GetByID(accountId);
            if (existedAccount == null)
            {
                throw new DataNotFoundException("Account is not found in database");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                if (isBlock)
                {
                    existedAccount.Status = 0;
                }
                else
                {
                    existedAccount.Status = 1;
                }
                existedAccount.UpdatedAt = DateTime.Now;
                await _accountRepository.UpdateAsync(existedAccount);
                _unitOfWork.CommitTransaction();
                _unitOfWork.Save();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InternalServerErrorException($"An Internal error occurred while creating staff: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<PaginationList<StaffAccountResponse>> GetPaginationStaffAccount(int pageSize, int pageIndex)
        {
            try
            {
                //Guid roleIdGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440201");
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "STAFF")).FirstOrDefault();
                if (role == null)
                {
                    throw new DataNotFoundException("Failed to get role info");
                }
                var roleIdGuid = role.RoleId;
                var accountIEnumerable = await _accountRepository.GetAsync(
                    filter: a => a.RoleId.Equals(roleIdGuid),
                    includeProperties: "Bar");
                var total = accountIEnumerable.Count();

                int validPageIndex = pageIndex > 0 ? pageIndex - 1 : 0;
                int validPageSize = pageSize > 0 ? pageSize : 10;
                accountIEnumerable = accountIEnumerable.Skip(validPageIndex * validPageSize).Take(validPageSize);

                var items = _mapper.Map<IEnumerable<StaffAccountResponse>>(accountIEnumerable);
                if (items == null || !items.Any())
                {
                    throw new DataNotFoundException("Staff's accounts is empty list");
                }
                var result = new PaginationList<StaffAccountResponse>
                {
                    items = items,
                    total = items.Count(),
                    //pageIndex = pageIndex,
                    //pageSize = pageSize
                };
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"An Internal error occurred: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }
        public async Task<PaginationList<CustomerAccountResponse>> GetPaginationCustomerAccount(int pageSize, int pageIndex)
        {
            try
            {
                //Guid roleIdGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440202");
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "CUSTOMER")).FirstOrDefault();
                if (role == null)
                {
                    throw new DataNotFoundException("Failed to get role info");
                }
                var roleIdGuid = role.RoleId;
                var accountIEnumerable = await _accountRepository.GetAsync(
                    filter: a => a.RoleId.Equals(roleIdGuid));
                var total = accountIEnumerable.Count();

                int validPageIndex = pageIndex > 0 ? pageIndex - 1 : 0;
                int validPageSize = pageSize > 0 ? pageSize : 10;
                accountIEnumerable = accountIEnumerable.Skip(validPageIndex * validPageSize).Take(validPageSize);

                var items = _mapper.Map<IEnumerable<CustomerAccountResponse>>(accountIEnumerable);
                var result = new PaginationList<CustomerAccountResponse>
                {
                    items = items,
                    total = total,
                    //pageIndex = pageIndex,
                    //pageSize = pageSize
                };
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"An Internal error occurred: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<CustomerAccountResponse> GetCustomerAccountById(Guid accountId)
        {
            try
            {
                //Guid roleIdGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440202");
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "CUSTOMER")).FirstOrDefault();
                if (role == null)
                {
                    throw new DataNotFoundException("Failed to get role info");
                }
                var roleIdGuid = role.RoleId;
                var customerAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == accountId
                        && a.RoleId == roleIdGuid)).FirstOrDefault();
                if (customerAccount == null)
                {
                    throw new DataNotFoundException("Customer's account not found");
                }
                var result = _mapper.Map<CustomerAccountResponse>(customerAccount);
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"An Internal error occurred: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<StaffAccountResponse> GetStaffAccountById(Guid accountId)
        {
            try
            {
                //Guid roleIdGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440201");
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "STAFF")).FirstOrDefault();
                if (role == null)
                {
                    throw new DataNotFoundException("Failed to get role info");
                }
                var roleIdGuid = role.RoleId;
                var staffAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == accountId && a.RoleId == roleIdGuid,
                        includeProperties: "Bar"))
                    .FirstOrDefault();
                if (staffAccount == null)
                {
                    throw new DataNotFoundException("Staff's account not found");
                }
                var result = _mapper.Map<StaffAccountResponse>(staffAccount);
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"An Internal error occurred: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<CustomerInfoResponse> GetCustomerInfoById(Guid accountId)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    throw new CustomException.DataNotFoundException("Khách hàng không tồn tại.");
                }
                var role = await _unitOfWork.RoleRepository.GetByIdAsync(account.RoleId);
                if (role.RoleName != "CUSTOMER")
                {
                    throw new ForbbidenException("Bạn không thể truy cập");
                }
                var response = _mapper.Map<CustomerInfoResponse>(account);
                return response;
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task UpdateCustomerAccountByCustomer(Guid accountId, CustomerInfoRequest request)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    throw new CustomException.DataNotFoundException("Khách hàng không tồn tại.");
                }
                var role = await _unitOfWork.RoleRepository.GetByIdAsync(account.RoleId);
                if (role.RoleName != "CUSTOMER")
                {
                    throw new ForbbidenException("Bạn không thể truy cập");
                }
                var updatedAccount = _mapper.Map(request, account);
                updatedAccount.UpdatedAt = DateTime.Now;
                await _unitOfWork.AccountRepository.UpdateAsync(updatedAccount);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<string> UpdateCustomerAvatar(Guid accountId, IFormFile Image)
        {
            try
            {
                string imageUrl = "default";
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    throw new CustomException.DataNotFoundException("Tài khoản không tồn tại");
                }

                // upload image
                var fileToUpload = Utils.CheckValidateSingleImageFile(Image);

                imageUrl = await _fireBase.UploadImageAsync(Image);

                account.Image = imageUrl;
                await _unitOfWork.AccountRepository.UpdateAsync(account);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();
                return imageUrl;
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        //private static Guid EncryptAccountId(Guid accountId)
        //{
        //    string accountIdString = accountId.ToString();

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Encoding.UTF8.GetBytes(key);
        //        aes.IV = new byte[16];

        //        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter sw = new StreamWriter(cs))
        //                {
        //                    sw.Write(accountIdString);
        //                }
        //            }

        //            // Chuyển kết quả mã hóa thành chuỗi Base64
        //            string encryptedBase64 = Convert.ToBase64String(ms.ToArray());

        //            // Giới hạn độ dài chuỗi Base64 để phù hợp với độ dài của GUID (32 ký tự)
        //            string truncatedBase64 = encryptedBase64.Replace("=", "").Replace("/", "").Replace("+", "").Substring(0, 32);

        //            return new Guid(truncatedBase64);
        //        }
        //    }
        //}

        //private static Guid DecryptAccountId(string hashedAccountId)
        //{
        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Encoding.UTF8.GetBytes(key);
        //        aes.IV = new byte[16];

        //        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        //        using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(hashedAccountId)))
        //        {
        //            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (StreamReader sr = new StreamReader(cs))
        //                {
        //                    string decryptedString = sr.ReadToEnd();
        //                    return Guid.Parse(decryptedString);
        //                }
        //            }
        //        }
        //    }
        //}

    }
}
