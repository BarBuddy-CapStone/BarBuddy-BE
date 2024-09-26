using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Table")]
    public class Table
    {
        [Key]
        public string TableId { get; set; }
        public string BarId { get; set; }
        public string TableTypeId { get; set; }
        public string TableName { get; set; }
        public int Status { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<BookingTable> BookingTables { get; set; }

        [ForeignKey("TableTypeId")]
        public virtual TableType TableType { get; set; }

        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }
    }
}
