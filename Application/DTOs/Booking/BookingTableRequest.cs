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
        public List<Guid>? TableIds { get; set; }

        public static ValidationResult? ValidateBookingDate(DateTimeOffset bookingDate, ValidationContext context)
        {
            return bookingDate < DateTimeOffset.Now.Date ? new ValidationResult("Ngày đặt sai") : ValidationResult.Success;
        }
    }
}
