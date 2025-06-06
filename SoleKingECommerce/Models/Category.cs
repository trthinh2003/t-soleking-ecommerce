using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? ParentId { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
