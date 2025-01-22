using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Shembull: Order i përket një përdoruesi
        public string? UserId { get; set; }
        // Opsionale: public ApplicationUser? User { get; set; }
    }
}