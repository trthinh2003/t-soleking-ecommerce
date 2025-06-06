namespace SoleKingECommerce.Models
{
    public class Wishlist
    {
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}