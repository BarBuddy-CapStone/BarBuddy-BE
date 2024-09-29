using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("DrinkEmotionalCategory")]
    public class DrinkEmotionalCategory
    {
        [Key]
        public Guid DrinkEmotionalCategoryId  { get; set; } = Guid.NewGuid();
        public Guid EmotionalDrinkCategoryId { get; set; }
        public Guid DrinkId { get; set; }

        [ForeignKey("EmotionalDrinkCategoryId")]
        public virtual EmotionalDrinkCategory EmotionalDrinkCategory { get; set; }

        [ForeignKey("DrinkId")]
        public virtual Drink Drink { get; set; }
    }
}
