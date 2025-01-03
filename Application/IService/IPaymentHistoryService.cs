﻿using Application.DTOs.PaymentHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IPaymentHistoryService
    {
        Task<(List<PaymentHistoryResponse> response, int totalPage)> Get(int Status, string? CustomerName, string? PhoneNumber, string? Email, Guid? BarId, DateTime? PaymentDate, int PageIndex, int PageSize);
        Task<(List<PaymentHistoryByCustomerResponse> response, int totalPage)> GetByCustomerId(Guid customerId, int? Status, int PageIndex, int PageSize);
        Task<(List<PaymentHistoryResponse> response, int totalPage)> GetByBarId(Guid barId, int pageIndex, int pageSize);
    }
}
