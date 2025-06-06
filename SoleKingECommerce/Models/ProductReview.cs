namespace SoleKingECommerce.Models
{
    public class ProductReview
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}