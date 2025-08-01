namespace SoleKingECommerce.ViewModels.Import
{
    public class OriginalImportViewModel
    {
        public int Id { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<AddedItemViewModel> AddedItems { get; set; } = new List<AddedItemViewModel>();
    }
}
