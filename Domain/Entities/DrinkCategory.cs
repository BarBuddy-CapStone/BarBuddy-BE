using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("DrinkCategory")]
    public class DrinkCategory
    {
        [Key]
        public Guid DrinksCategoryId {  get; set; } = Guid.NewGuid();
        public string DrinksCategoryName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Drink> Drinks { get; set; }
    }
}
