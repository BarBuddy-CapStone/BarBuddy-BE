using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Drink")]
    public class Drink
    {
        [Key]
        public Guid DrinkId { get; set; } = Guid.NewGuid();
        public string DrinkCode { get; set; }
        public Guid BarId { get; set; }
        public Guid DrinkCategoryId { get; set; }
        public string DrinkName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image {  get; set; }
        public bool Status { get; set; }

        public virtual ICollection<BookingDrink> BookingDrinks { get; set; }
        public virtual ICollection<DrinkEmotionalCategory> DrinkEmotionalCategories { get; set; }

        [ForeignKey("DrinkCategoryId")]
        public virtual DrinkCategory DrinkCategory { get; set; }
        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }

    }
}
