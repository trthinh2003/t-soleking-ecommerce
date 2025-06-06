namespace SoleKingECommerce.Models
{
    public class ImportItem
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public int ImportId { get; set; }
        public Import? Import { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }
    }
}