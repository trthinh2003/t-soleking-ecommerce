using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class UserPurchaseHistory
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime PurchasedAt { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
