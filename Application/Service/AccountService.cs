using Application.Common;
using Application.DTOs.Account;
using Application.DTOs.Bar;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Azure;
using Azure.Core;
using Domain.Common;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                throw new DataExistException("Email đã tồn tại, vui lòng thử lại !");
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
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
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
                throw new DataExistException("Email đã tồn tại, vui lòng thử lại !");
            }
            var staffRole = (await _unitOfWork.RoleRepository.GetAsync(filter: r => r.RoleName == "STAFF")).FirstOrDefault();
            var existedBar = await _barRepository.GetByIdAsync(request.BarId);
            if (existedBar == null)
            {
                throw new DataNotFoundException("Không tìm thấy quán bar !");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var newAccount = _mapper.Map<Account>(request);
                newAccount.RoleId = staffRole.RoleId;
                newAccount.Status = 1;
                newAccount.CreatedAt = DateTime.Now;
                newAccount.Password = await HashPassword("string");
                _accountRepository.Insert(newAccount);
                _unitOfWork.CommitTransaction();
                _unitOfWork.Save();
                var result = _mapper.Map<StaffAccountResponse>(newAccount);
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<ManagerAccountResponse> CreateManagerAccount(ManagerAccountRequest request)
        {
            var existedAccount = (await _accountRepository.GetAsync(a => a.Email.Equals(request.Email))).FirstOrDefault();
            if (existedAccount != null)
            {
                throw new DataExistException("Tài khoản không tìm thấy");
            }
            var managerRole = (await _unitOfWork.RoleRepository.GetAsync(filter: r => r.RoleName == "MANAGER")).FirstOrDefault();
            var existedBar = await _barRepository.GetByIdAsync(request.BarId);
            if (existedBar == null)
            {
                throw new DataNotFoundException("Dữ liệu quán Bar không có trong Database");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var newAccount = _mapper.Map<Account>(request);
                newAccount.RoleId = managerRole.RoleId;
                newAccount.Status = 1;
                newAccount.CreatedAt = DateTime.Now;
                newAccount.Password = await HashPassword(RandomString(10));
                _accountRepository.Insert(newAccount);
                var result = _mapper.Map<ManagerAccountResponse>(newAccount);
                _unitOfWork.CommitTransaction();
                _unitOfWork.Save();
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
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
                throw new DataNotFoundException("Tài khoản không tìm thấy");
            }
            else if (existedAccount.Email != request.Email)
            {
                throw new DataNotFoundException("Tài khoản bị sai");
            }
            if (existedBar == null)
            {
                throw new DataNotFoundException("Quán Bar không tìm thấy");
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
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<ManagerAccountResponse> UpdateManagerAccount(Guid accountId, ManagerAccountRequest request)
        {
            var existedAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == accountId && a.Role.RoleName == "MANAGER"))
                .FirstOrDefault();
            var existedBar = await _barRepository.GetByIdAsync(request.BarId);
            if (existedAccount == null)
            {
                throw new DataNotFoundException("Tài khoản không tìm thấy");
            }
            else if (existedAccount.Email != request.Email)
            {
                throw new DataNotFoundException("Tài khoản bị sai");
            }
            if (existedBar == null)
            {
                throw new DataNotFoundException("Quán Bar không tìm thấy");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var updatedAccount = _mapper.Map(request, existedAccount);
                updatedAccount.UpdatedAt = DateTime.Now;
                _accountRepository.Update(updatedAccount);
                var result = _mapper.Map<ManagerAccountResponse>(updatedAccount);
                _unitOfWork.CommitTransaction();
                _unitOfWork.Save();
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
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
                throw new DataNotFoundException("Tài khoản không tìm thấy");
            }
            else if (existedAccount.Email != request.Email)
            {
                throw new DataNotFoundException("Tài khoản bị sai");
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
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
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
                throw new DataNotFoundException("Tài khoản không tìm thấy");
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
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<PaginationList<StaffAccountResponse>> GetPaginationStaffAccount(int pageSize, int pageIndex, Guid? barId = null)
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
                IEnumerable<Account> accountIEnumerable;
                if (barId is not null)
                {
                    accountIEnumerable = await _accountRepository.GetAsync(
                    filter: a => a.RoleId.Equals(roleIdGuid) && a.BarId.Equals(barId),
                    includeProperties: "Bar");
                }
                else
                {
                    accountIEnumerable = await _accountRepository.GetAsync(
                    filter: a => a.RoleId.Equals(roleIdGuid),
                    includeProperties: "Bar");
                }

                var total = accountIEnumerable.Count();

                int validPageIndex = pageIndex > 0 ? pageIndex - 1 : 0;
                int validPageSize = pageSize > 0 ? pageSize : 10;
                accountIEnumerable = accountIEnumerable.Skip(validPageIndex * validPageSize).Take(validPageSize);

                var items = _mapper.Map<IEnumerable<StaffAccountResponse>>(accountIEnumerable);
                if (items == null || !items.Any())
                {
                    throw new DataNotFoundException("Danh sách nhân viên đang trống !");
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
                throw new DataNotFoundException(ex.Message);
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<PagingManagerAccountResponse> GetPaginationManagerAccount(ObjectQuery query)
        {
            try
            {
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "MANAGER")).FirstOrDefault();
                if (role == null)
                {
                    throw new DataNotFoundException("Thất bại khi phân quyền !");
                }

                Expression<Func<Account, bool>> filter = null;
                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    filter = account => account.RoleId.Equals(role.RoleId) &&
                                      account.Fullname.Contains(query.Search);
                }
                else
                {
                    filter = account => account.RoleId.Equals(role.RoleId);
                }

                var accounts = (await _accountRepository.GetAsync(
                    filter: filter,
                    includeProperties: "Bar"))
                    .ToList();

                if (!accounts.Any())
                {
                    throw new DataNotFoundException("Danh sách nhân viên đang trống");
                }

                var response = _mapper.Map<List<ManagerAccountResponse>>(accounts);

                var pageIndex = query.PageIndex ?? 1;
                var pageSize = query.PageSize ?? 6;

                var totalItems = response.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var paginatedAccounts = response.Skip((pageIndex - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToList();

                return new PagingManagerAccountResponse
                {
                    ManagerAccountResponses = paginatedAccounts,
                    TotalPages = totalPages,
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<PagingCustomerAccountResponse> GetPaginationCustomerAccount(ObjectQuery query)
        {
            try
            {
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "CUSTOMER")).FirstOrDefault();
                if (role == null)
                {
                    throw new DataNotFoundException("Thất bại khi phân quyền !");
                }

                Expression<Func<Account, bool>> filter = null;
                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    filter = account => account.RoleId.Equals(role.RoleId) &&
                                      account.Fullname.Contains(query.Search);
                }
                else
                {
                    filter = account => account.RoleId.Equals(role.RoleId);
                }

                var accounts = (await _accountRepository.GetAsync(filter: filter))
                                .ToList();

                if (!accounts.Any())
                {
                    throw new DataNotFoundException("Không tìm thấy tài khoản khách hàng nào!");
                }

                var response = _mapper.Map<List<CustomerAccountResponse>>(accounts);

                var pageIndex = query.PageIndex ?? 1;
                var pageSize = query.PageSize ?? 6;

                var totalItems = response.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var paginatedAccounts = response.Skip((pageIndex - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToList();

                return new PagingCustomerAccountResponse
                {
                    CustomerAccounts = paginatedAccounts,
                    TotalPages = totalPages,
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
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
                    throw new DataNotFoundException("Thất bại khi phân quyền !");
                }
                var roleIdGuid = role.RoleId;
                var customerAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == accountId
                        && a.RoleId == roleIdGuid)).FirstOrDefault();
                if (customerAccount == null)
                {
                    throw new DataNotFoundException("Không tìm thấy khách hàng !");
                }
                var result = _mapper.Map<CustomerAccountResponse>(customerAccount);
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"Thất bại khi phân quyền !: {ex.Message}");
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
                    throw new DataNotFoundException("Thất bại khi phân quyền !");
                }
                var roleIdGuid = role.RoleId;
                var staffAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == accountId && a.RoleId == roleIdGuid,
                        includeProperties: "Bar"))
                    .FirstOrDefault();
                if (staffAccount == null)
                {
                    throw new DataNotFoundException("Không tìm thấy nhân viên bạn đang tìm !");
                }
                var result = _mapper.Map<StaffAccountResponse>(staffAccount);
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<ManagerAccountResponse> GetManagerAccountById(Guid accountId)
        {
            try
            {
                //Guid roleIdGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440201");
                var role = (await _unitOfWork.RoleRepository.GetAsync(r => r.RoleName == "MANAGER")).FirstOrDefault();
                if (role == null)
                {
                    throw new DataNotFoundException("Thất bại khi phân quyền !");
                }
                var roleIdGuid = role.RoleId;
                var managerAccount = (await _accountRepository.GetAsync(filter: a => a.AccountId == accountId && a.RoleId == roleIdGuid,
                        includeProperties: "Bar"))
                    .FirstOrDefault();
                if (managerAccount == null)
                {
                    throw new DataNotFoundException("Tài khoản không tìm thấy");
                }
                var result = _mapper.Map<ManagerAccountResponse>(managerAccount);
                return result;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException($"Lỗi hệ thống: {ex.Message}");
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
            catch (DataNotFoundException ex)
            {
                throw new DataNotFoundException(ex.Message);
            }
            catch (ForbbidenException ex)
            {
                throw new ForbbidenException(ex.Message);
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

        public async Task<CustomerAccountResponse> UpdateCustomerSts(UpdCustomerStsRequest request)
        {
            try
            {
                var getOneCustomer = (await _accountRepository
                                                .GetAsync(filter: x => x.AccountId.Equals(request.AccountId)
                                                                    && !x.BarId.HasValue
                                                                    && x.Role.RoleName.Equals(PrefixKeyConstant.CUSTOMER),
                                                                    includeProperties: "Role"))
                                                                    .FirstOrDefault();

                if (getOneCustomer == null)
                {
                    throw new DataNotFoundException("Không tìm thấy khách hàng !");
                }
                getOneCustomer.Status = request.Status;
                await _unitOfWork.AccountRepository.UpdateRangeAsync(getOneCustomer);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();

                var response = _mapper.Map<CustomerAccountResponse>(getOneCustomer);
                return response;
            }
            catch (InternalServerErrorException ex)
            {
                throw new InternalServerErrorException(ex.Message);
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
