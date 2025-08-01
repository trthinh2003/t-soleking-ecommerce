namespace SoleKingECommerce.ViewModels.Voucher
{
    public class VoucherStatsViewModel
    {
        public int TotalVouchers { get; set; }
        public int ActiveVouchers { get; set; }
        public int ExpiredVouchers { get; set; }
        public int UpcomingVouchers { get; set; }
    }
}
