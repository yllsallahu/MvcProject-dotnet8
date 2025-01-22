using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // For simplicity, let's say each order has 1 product
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // Optional: many properties e.g. CustomerName, etc.
        public string? CustomerName { get; set; }

        public bool IsActive { get; set; } = true;

        // Who created the order
        public string? CreatedByUserId { get; set; }
    }
}
