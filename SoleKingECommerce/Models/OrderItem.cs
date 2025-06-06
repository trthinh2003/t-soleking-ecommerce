namespace SoleKingECommerce.Models
{
    public class OrderItem
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }
    }
}