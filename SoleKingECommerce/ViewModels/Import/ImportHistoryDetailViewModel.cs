namespace SoleKingECommerce.ViewModels.Import
{
    public class ImportHistoryDetailViewModel
    {
        public int Id { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierPhone { get; set; } = string.Empty;
        public string SupplierEmail { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string? Note { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ImportItemDetailsViewModel> Items { get; set; } = new List<ImportItemDetailsViewModel>();

        // Reference information
        public bool IsFromReference { get; set; }
        public OriginalImportViewModel? OriginalImport { get; set; }
        public List<RelatedImportViewModel> RelatedImports { get; set; } = new List<RelatedImportViewModel>();
    }
}
