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
        public string EmotionalDrinksCategoryId {  get; set; }
        public string CategoryName { get; set; }
    }
}
