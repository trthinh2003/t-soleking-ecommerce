namespace SoleKingECommerce.ViewModels.Import
{
    public class AddedItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int QuantityAdded { get; set; }
        public string? Note { get; set; }
    }
}
