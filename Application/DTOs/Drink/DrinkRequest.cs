using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Drink
{
    public class DrinkRequest
    {
        [Required]
        public Guid DrinkCategoryId { get; set; }
        [Required]
        public Guid BarId { get; set; }
        [Required]
        public string DrinkName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public List<Guid> DrinkBaseEmo {  get; set; }
        public List<IFormFile>? Images { get; set; }
        public List<string>? OldImages { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}
