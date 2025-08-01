namespace SoleKingECommerce.ViewModels.ProductVariant
{
    public class ProductVariantEditViewModel
    {
        public int Id { get; set; }
        public string? Size { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? SKU { get; set; }
        public int ColorId { get; set; }
        public string? ColorName { get; set; }
        public bool IsDeleted { get; set; }
    }
}
