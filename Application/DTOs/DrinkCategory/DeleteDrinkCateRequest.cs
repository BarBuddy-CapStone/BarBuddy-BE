using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DrinkCategory
{
    public class DeleteDrinkCateRequest
    {
        [Required]
        public bool IsDeleted { get; set; }
    }
}
