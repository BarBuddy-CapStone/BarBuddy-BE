﻿using Application.DTOs.BarTime;
using Application.DTOs.Response.FeedBack;
using Application.DTOs.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class BarResponse
    {
        public Guid BarId { get; set; }
        public string BarName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Images { get; set; }
        public double Discount { get; set; }
        public bool Status { get; set; }
        public double TimeSlot { get; set; }
        public bool IsAnyTableAvailable { get; set; }
        public ICollection<FeedBackResponse>? FeedBacks { get; set; }
        public ICollection<TableResponse>? Tables { get; set; }
        public ICollection<BarTimeResponse>? BarTimeResponses { get; set; }
    }
}
