using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string? Code { get; set; }
        public string? Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal MaxDiscount { get; set; }
        public decimal MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }

        public ICollection<VoucherUsage>? VoucherUsages { get; set; }
    }
}