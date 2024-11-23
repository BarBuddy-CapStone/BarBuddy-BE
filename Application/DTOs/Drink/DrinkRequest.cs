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
        public Guid BarId { get; set; }
        [Required]
        public Guid DrinkCategoryId { get; set; }
        [Required(ErrorMessage = "Tên đồ uống không thể trống !")]
        [StringLength(50, MinimumLength = 7, ErrorMessage = "Tên đồ uống phải từ 7 đến 50 kí tự !")]
        public string DrinkName { get; set; }
        [Required(ErrorMessage = "Mô tả không thể trống !")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Mô tả phải từ 7 đến 100 kí tự !")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Giá không thể trống !")]
        [Range(0,100000000, ErrorMessage = "Giá đồ uống phải từ 0 đến 100000000 !")]
        public double Price { get; set; }
        [Required]
        public List<Guid> DrinkBaseEmo {  get; set; }
        public List<IFormFile>? Images { get; set; }
        public List<string>? OldImages { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}
