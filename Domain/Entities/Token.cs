using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Token
    {
        [Key]
        public Guid TokenId { get; set; } = Guid.NewGuid();
        public Guid AccountId { get; set; }
        public string Tokens {  get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}
