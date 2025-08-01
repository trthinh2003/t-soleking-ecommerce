namespace SoleKingECommerce.Models
{
    public class ImportReferenceItem
    {
        public int Id { get; set; }

        public int ImportReferenceId { get; set; }
        public int ProductVariantId { get; set; }

        public int QuantityAdded { get; set; }

        public string? Note { get; set; }

        public ImportReference ImportReference { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}