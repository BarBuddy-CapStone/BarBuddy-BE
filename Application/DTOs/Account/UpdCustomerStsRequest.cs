using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Account
{
    public class UpdCustomerStsRequest
    {
        [Required(ErrorMessage = "Tài khoản không được phép trống !")]
        public Guid AccountId { get; set; }
        [Required(ErrorMessage ="Trạng thái tài khoản không được phép trống !")]
        [Range(0,1, ErrorMessage = "Trạng thái tài khoản chỉ có 0 và 1 !")]
        public int Status {  get; set; }
    }
}
