using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MvcProject.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public int ProductId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public const string StatusPending = "Pending";
        public const string StatusCancelled = "Cancelled";
        public const string StatusReceived = "Received";

        public string Status { get; set; } = StatusPending;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Product Product { get; set; }
        public IdentityUser User { get; set; }
    }
}