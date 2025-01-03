﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Authen
{
    public class LoginResponse
    {
        public Guid AccountId { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Image { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public Guid? IdentityId { get; set; }
        public string BarName { get; set; }
    }
}
