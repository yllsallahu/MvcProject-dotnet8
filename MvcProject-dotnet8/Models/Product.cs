using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true; 

       
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        
        public string? CreatedByUserId { get; set; }
        
        // Add navigation property for permissions
        public ICollection<ProductPermission> Permissions { get; set; } = new List<ProductPermission>();
    }
}
