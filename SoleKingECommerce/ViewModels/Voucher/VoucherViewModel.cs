namespace SoleKingECommerce.ViewModels.Voucher
{
    public class VoucherViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal MaxDiscount { get; set; }
        public decimal MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}
