using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(250)]
        public string? Name { get; set; }

        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        public string? Description { get; set; }

        public decimal? BasePrice { get; set; }

        public string? Tags { get; set; }

        public string? Slug { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<DiscountCondition>? DiscountConditions { get; set; } = new List<DiscountCondition>();
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<ProductDiscount>? ProductDiscounts { get; set; } = new List<ProductDiscount>();

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}