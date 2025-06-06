using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class DiscountCondition
    {
        [Key]
        public int Id { get; set; }
        public int MinQuantity { get; set; }
        public decimal PercentDiscount { get; set; }
        public int GiftProductQuantity { get; set; }

        public int DiscountId { get; set; }
        public Discount? Discount { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}