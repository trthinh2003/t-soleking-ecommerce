namespace SoleKingECommerce.ViewModels.Import
{
    public class ImportListViewModel
    {
        public int Id { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public string? Note { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
