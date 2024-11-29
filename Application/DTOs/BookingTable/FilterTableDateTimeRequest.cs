using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.BookingTable
{
    public class FilterTableDateTimeRequest
    {
        [Required]
        public Guid BarId { get; set; }

        public Guid TableTypeId { get; set; }

        [Required]
        public DateTimeOffset Date { get; set; }
        [Required]
        public TimeSpan Time { get; set; }
    }
}
