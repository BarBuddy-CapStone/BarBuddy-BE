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
        public string DrinkEmotionalCategoryId  { get; set; }
        public string EmotionalDrinkCategoryId { get; set; }
        public string DrinkId { get; set; }


        [ForeignKey("EmotionalDrinkCategoryId")]
        public virtual EmotionalDrinkCategory EmotionalDrinkCategory { get; set; }

        [ForeignKey("DrinkId")]
        public virtual Drink Drink { get; set; }
    }
}
