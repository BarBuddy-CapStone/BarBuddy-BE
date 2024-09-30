using Application.DTOs.Account;

namespace Application.IService
{
    public interface IAccountService
    {
        Task<bool> BlockOrUnblockAccount(string accountId, bool isBlock);
        Task<CustomerAccountResponse> CreateCustomerAccount(CustomerAccountRequest request);
        Task<StaffAccountResponse> CreateStaffAccount(StaffAccountRequest request);
        Task<CustomerAccountResponse> GetCustomerAccountById(Guid accountId);
        Task<PaginationList<CustomerAccountResponse>> GetPaginationCustomerAccount(int pageSize, int pageIndex);
        Task<PaginationList<StaffAccountResponse>> GetPaginationStaffAccount(int pageSize, int pageIndex);
        Task<StaffAccountResponse> GetStaffAccountById(Guid accountId);
        Task<CustomerAccountResponse> UpdateCustomerAccount(Guid accountId, CustomerAccountRequest request);
        Task<StaffAccountResponse> UpdateStaffAccount(Guid accountId, StaffAccountRequest request);
    }
}