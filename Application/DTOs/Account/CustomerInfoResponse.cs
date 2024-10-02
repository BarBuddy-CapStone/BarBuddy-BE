using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Account
{
    public class CustomerInfoResponse
    {
        public Guid AccountId { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        [DataType(DataType.Date)]
        public DateTimeOffset Dob { get; set; }
        public string Image { get; set; }
    }
}
