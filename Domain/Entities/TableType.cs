using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("TableType")]
    public class TableType
    {
        [Key]
        public Guid TableTypeId { get; set; } = Guid.NewGuid();
        public string TypeName { get; set; }
        public string Description { get; set; }
        public int MinimumGuest {  get; set; }
        public int MaximumGuest { get; set; }
        public double MinimumPrice { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Table> Tables { get; set;}
    }
}
