using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(150)]
        public string? CustomerName { get; set; }

        [MaxLength(15)]
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public decimal TotalPrice { get; set; }

        [MaxLength(100)]
        public string? Status { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        public ICollection<OrderItem>? Items { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<VoucherUsage>? VoucherUsages { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}