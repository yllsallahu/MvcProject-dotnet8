using System.ComponentModel.DataAnnotations;

namespace MyMVCProject.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = default!;

        public bool IsActive { get; set; } = true;
    }
}