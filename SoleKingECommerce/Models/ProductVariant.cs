using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        [MaxLength(50)]
        public string? SKU { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int ColorId { get; set; }
        public Color? Color { get; set; }

        public ICollection<ImportItem>? ImportItems { get; set; } = new List<ImportItem>();
        public ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem>? CartItems { get; set; } = new List<CartItem>();

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}