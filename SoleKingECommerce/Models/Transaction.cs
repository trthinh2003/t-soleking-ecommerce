using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public string? PaymentMethod { get; set; } = "COD";
        public string? PaymentStatus { get; set; } = "Pending";
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}