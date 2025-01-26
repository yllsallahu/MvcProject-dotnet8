using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public string? UserId { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}