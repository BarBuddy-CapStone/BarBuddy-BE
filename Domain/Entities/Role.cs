using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace Domain.Entities
{
    [Table("Role")]
    public class Role
    {
        [Key]
        public Guid RoleId { get; set; } = Guid.NewGuid();
        public string RoleName { get; set; }
        public virtual ICollection<Account> Accounts { get; set;}
    }
}
