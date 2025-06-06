using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        public string? ImageUrl { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}