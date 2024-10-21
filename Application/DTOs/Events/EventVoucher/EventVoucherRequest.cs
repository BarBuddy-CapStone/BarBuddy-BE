using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Events.EventVoucher
{
    public class EventVoucherRequest
    {
        [Required]
        public string EventVoucherName { get; set; }
        [Required]
        [MinLength(10, ErrorMessage = "Độ dài tối thiểu là 10 kí tự")]
        [MaxLength(12, ErrorMessage = "Độ dài tối đa là 12 kí tự")]
        public string VoucherCode { get; set; }
        [Required]
        [Range(0, 100, ErrorMessage = "Thấp nhất là 0% và cao nhất là 100%")]
        public double Discount { get; set; }
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 1")]
        public double MaxPrice { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng voucher phải lớn hơn 1")]
        public int Quantity { get; set; }
    }
}
