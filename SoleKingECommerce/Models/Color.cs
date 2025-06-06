using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Color
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Hex { get; set; }

        public ICollection<ProductVariant> ProductVariants { get; set; } = new HashSet<ProductVariant>();
    }
}