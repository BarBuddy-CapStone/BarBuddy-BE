﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PaymentHistory
{
    public class PaymentHistoryResponse
    {
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? BarName { get; set; }
        public string? TransactionCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }
        public bool Status { get; set; }
    }
}
