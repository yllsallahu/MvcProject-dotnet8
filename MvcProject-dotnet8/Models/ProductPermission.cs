using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models
{
    public class ProductPermission
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; } = default!;
        public bool CanView { get; set; } = true;
        public bool CanEdit { get; set; } = false;
        
        // Navigation properties
        public Product Product { get; set; } = default!;
    }
}
