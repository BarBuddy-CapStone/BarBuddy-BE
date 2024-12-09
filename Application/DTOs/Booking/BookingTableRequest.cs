using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Booking
{
    public class BookingTableRequest
    {
        [Required]
        public Guid BarId { get; set; }
        [CustomValidation(typeof(BookingTableRequest), "ValidateBookingDate", ErrorMessage = "Ngày đặt sai")]
        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public string? Note { get; set; }
        [Required]
        [MinLength(1, ErrorMessage ="Bạn phải chọn ít nhất 1 bàn để đặt !")]
        [MaxLength(5, ErrorMessage ="Bạn chỉ được đặt tối đa 5 bàn !")]
        public List<Guid>? TableIds { get; set; }
        public int NumOfPeople { get; set; }

        public static ValidationResult? ValidateBookingDate(DateTime bookingDate, ValidationContext context)
        {
            return bookingDate < DateTime.Now.Date ? new ValidationResult("Ngày đặt sai") : ValidationResult.Success;
        }
    }
}
