namespace SoleKingECommerce.ViewModels.Import
{
    public class ImportHistoryExportViewModel
    {
        public int ImportId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public string Note { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsFromReference { get; set; }
        public string OriginalImportId { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
    }
}
