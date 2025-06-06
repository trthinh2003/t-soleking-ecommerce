using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class VoucherUsage
    {
        [Key]
        public int Id { get; set; }
        public DateTime? UsedAt { get; set; }

        public int VoucherId { get; set; }
        public Voucher? Voucher { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}