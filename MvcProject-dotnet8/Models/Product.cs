using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Key
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]

        // User who created the product
        public string? CreatedByUserId { get; set; }
    }
}
