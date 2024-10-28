using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("EmotionalDrinkCategory")]
    public class EmotionalDrinkCategory
    {
        [Key]
        public Guid EmotionalDrinksCategoryId { get; set; } = Guid.NewGuid();
        public Guid BarId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }
    }
}
