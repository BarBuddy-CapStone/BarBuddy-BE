﻿using Application.DTOs.Bar;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Account
{
    public class StaffAccountResponse
    {
        public Guid AccountId { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Image {  get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        //public DateTimeOffset UpdatedAt { get; set; }
        [DataType(DataType.Date)]
        public DateTimeOffset Dob { get; set; }
        public string BarId { get; set; }
        public virtual BarResponse? Bar { get; set; }
        public int Status { get; set; }
    }
}
