using Application.DTOs.Account;
using Application.IService;
using AutoMapper;
using Azure.Core;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Domain.CustomException.CustomException;

namespace Application.Service
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly IGenericRepository<Bar> _barRepository;
        private static Random random;
        private static readonly string key = "1234567890123456";

        public AccountService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _accountRepository = _unitOfWork.GetRepository<Account>();
            _barRepository = _unitOfWork.GetRepository<Bar>();
            random = new Random();
        }

        public async Task<CustomerAccountResponse> CreateCustomerAccount(CustomerAccountRequest request)
        {
            var existedAccount = (await _accountRepository.GetAsync(a => a.Email.Equals(request.Email))).FirstOrDefault();
            if (existedAccount != null)
            {
                throw new DataExistException("Email is existed");
            }
            else
            {
                try
                {
                    _unitOfWork.BeginTransaction();
                    var newAccount = _mapper.Map<Account>(request);
                    newAccount.Status = 1;
                    newAccount.CreatedAt = DateTime.Now;
                    newAccount.Password = await HashPassword(RandomString(10));
                    _accountRepository.Insert(newAccount);
                    _unitOfWork.CommitTransaction();
                    _unitOfWork.Save();
                    var result = _mapper.Map<CustomerAccountResponse>(newAccount);
                    return result;
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
            }
        }

        public async Task<StaffAccountResponse> CreateStaffAccount(StaffAccountRequest request)
        {
            var existedAccount = (await _accountRepository.GetAsync(a => a.Email.Equals(request.Email))).FirstOrDefault();
            var existedBar = await _barRepository.GetByIdAsync(request.BarId);
            if (existedAccount != null)
            {
                throw new DataExistException("Email is existed");
            }
            if (existedBar == null)
            {
                throw new DataNotFoundException("Bar is not found in database");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var newAccount = _mapper.Map<Account>(request);
                newAccount.AccountId = new Guid().ToString();
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

        public async Task<StaffAccountResponse> UpdateStaffAccount(string accountId, StaffAccountRequest request)
        {
            var detachedAccountId = DecryptAccountId(accountId);
            var existedAccount = _accountRepository.GetByID(detachedAccountId);
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
                result.AccountId = EncryptAccountId(result.AccountId);
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

        public async Task<CustomerAccountResponse> UpdateCustomerAccount(string accountId, CustomerAccountRequest request)
        {
            var detachedAccountId = DecryptAccountId(accountId);
            var existedAccount = await _accountRepository.GetByIdAsync(detachedAccountId);
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
                result.AccountId = EncryptAccountId(result.AccountId);
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
                var accountIEnumerable = _accountRepository
                    .Get(filter: a => a.RoleId == "550e8400-e29b-41d4-a716-446655440201", 
                        pageSize: pageSize, pageIndex: pageIndex)
                    .Select(account =>
                    {
                        account.AccountId = EncryptAccountId(account.AccountId);
                        return account;
                    })
                    .ToList();
                var items = _mapper.Map<IEnumerable<StaffAccountResponse>>(accountIEnumerable);
                if (items == null || !items.Any())
                {
                    throw new DataNotFoundException("Staff's accounts is empty list");
                }
                var count = await _accountRepository.CountAsync();
                var result = new PaginationList<StaffAccountResponse>
                {
                    items = items,
                    count = count,
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
                var accountIEnumerable = _accountRepository
                    .Get(filter: a => a.RoleId == "550e8400-e29b-41d4-a716-446655440202", 
                        pageSize: pageSize, pageIndex: pageIndex)
                    .Select(account =>
                    {
                        account.AccountId = EncryptAccountId(account.AccountId);
                        return account;
                    })
                    .ToList();
                var items = _mapper.Map<IEnumerable<CustomerAccountResponse>>(accountIEnumerable);
                if (items == null || !items.Any())
                {
                    throw new DataNotFoundException("Customer's accounts is empty list");
                }
                var count = await _accountRepository.CountAsync();
                var result = new PaginationList<CustomerAccountResponse>
                {
                    items = items,
                    count = count,
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

        public async Task<StaffAccountResponse> GetStaffAccountByEmail(string email)
        {
            try
            {
                var staffAccount = (await _accountRepository.GetAsync(filter: a => a.Email == email
                        && a.RoleId == "550e8400-e29b-41d4-a716-446655440201")).FirstOrDefault();
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

        public async Task<CustomerAccountResponse> GetCustomerAccountByEmail(string email)
        {
            try
            {
                var customerAccount = (await _accountRepository.GetAsync(filter: a => a.Email == email
                        && a.RoleId == "550e8400-e29b-41d4-a716-446655440202")).FirstOrDefault();
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

        public async Task<CustomerAccountResponse> GetCustomerAccountById(string accountId)
        {
            try
            {
                var detachedAccountId = DecryptAccountId(accountId);
                var customerAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == detachedAccountId
                        && a.RoleId == "550e8400-e29b-41d4-a716-446655440202")).FirstOrDefault();
                if (customerAccount == null)
                {
                    throw new DataNotFoundException("Customer's account not found");
                }
                var result = _mapper.Map<CustomerAccountResponse>(customerAccount);
                result.AccountId = EncryptAccountId(result.AccountId);
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

        public async Task<CustomerAccountResponse> GetStaffAccountById(string accountId)
        {
            try
            {
                var detachedAccountId = DecryptAccountId(accountId);
                var staffAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == detachedAccountId
                        && a.RoleId == "550e8400-e29b-41d4-a716-446655440201")).FirstOrDefault();
                if (staffAccount == null)
                {
                    throw new DataNotFoundException("Staff's account not found");
                }
                var result = _mapper.Map<CustomerAccountResponse>(staffAccount);
                result.AccountId = EncryptAccountId(result.AccountId);
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

        private static string EncryptAccountId(string accountId)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[16]; // Sử dụng IV mặc định là các byte 0

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(accountId);
                        }
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private static string DecryptAccountId(string hashedAccountId)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[16]; // Sử dụng IV mặc định là các byte 0

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(hashedAccountId)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
