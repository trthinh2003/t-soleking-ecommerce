using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public string? SessionId { get; set; }

        public string UserId { get; set; }
        public User? User { get; set; }
        
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
