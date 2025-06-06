namespace SoleKingECommerce.Models
{
    public class CartItem
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public int CartId { get; set; }
        public Cart? Cart { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }
    }
}