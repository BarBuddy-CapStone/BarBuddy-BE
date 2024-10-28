using Application.DTOs.Account;
using Microsoft.AspNetCore.Http;

namespace Application.IService
{
    public interface IAccountService
    {
        Task<bool> BlockOrUnblockAccount(string accountId, bool isBlock);
        Task<CustomerAccountResponse> CreateCustomerAccount(CustomerAccountRequest request);
        Task<ManagerAccountResponse> CreateManagerAccount(ManagerAccountRequest request);
        Task<StaffAccountResponse> CreateStaffAccount(StaffAccountRequest request);
        Task<CustomerAccountResponse> GetCustomerAccountById(Guid accountId);
        Task<ManagerAccountResponse> GetManagerAccountById(Guid accountId);
        Task<CustomerInfoResponse> GetCustomerInfoById(Guid accountId);
        Task<PaginationList<CustomerAccountResponse>> GetPaginationCustomerAccount(int pageSize, int pageIndex);
        Task<PaginationList<ManagerAccountResponse>> GetPaginationManagerAccount(int pageSize, int pageIndex);
        Task<PaginationList<StaffAccountResponse>> GetPaginationStaffAccount(int pageSize, int pageIndex, Guid? barId);
        Task<StaffAccountResponse> GetStaffAccountById(Guid accountId);
        Task<CustomerAccountResponse> UpdateCustomerAccount(Guid accountId, CustomerAccountRequest request);
        Task UpdateCustomerAccountByCustomer(Guid accountId, CustomerInfoRequest request);
        Task<string> UpdateCustomerAvatar(Guid accountId, IFormFile Image);
        Task<ManagerAccountResponse> UpdateManagerAccount(Guid accountId, ManagerAccountRequest request);
        Task<StaffAccountResponse> UpdateStaffAccount(Guid accountId, StaffAccountRequest request);
    }
}