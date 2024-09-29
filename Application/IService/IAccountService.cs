using Application.DTOs.Account;

namespace Application.IService
{
    public interface IAccountService
    {
        Task<bool> BlockOrUnblockAccount(string accountId, bool isBlock);
        Task<CustomerAccountResponse> CreateCustomerAccount(CustomerAccountRequest request);
        Task<StaffAccountResponse> CreateStaffAccount(StaffAccountRequest request);
        Task<CustomerAccountResponse> GetCustomerAccountByEmail(string email);
        Task<PaginationList<CustomerAccountResponse>> GetPaginationCustomerAccount(int pageSize, int pageIndex);
        Task<PaginationList<StaffAccountResponse>> GetPaginationStaffAccount(int pageSize, int pageIndex);
        Task<StaffAccountResponse> GetStaffAccountByEmail(string email);
        Task<CustomerAccountResponse> UpdateCustomerAccount(string accountId, CustomerAccountRequest request);
        Task<StaffAccountResponse> UpdateStaffAccount(string accountId, StaffAccountRequest request);
        Task<CustomerAccountResponse> GetCustomerAccountById(string accountId);
        Task<CustomerAccountResponse> GetStaffAccountById(string accountId);
    }
}