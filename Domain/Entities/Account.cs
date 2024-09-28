using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Account")]
    public class Account
    {
        [Key]
        public string AccountId { get; set; }
        public string? BarId {  get; set; }
        public string RoleId {  get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public DateTimeOffset Dob {  get; set; }
        public string Phone {  get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string Image { get; set; }
        public int Status {  get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<PaymentHistory> PaymentHistories { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }

    }
}
