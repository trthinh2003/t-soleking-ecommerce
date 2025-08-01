using SoleKingECommerce.ViewModels.Cart;

namespace SoleKingECommerce.ViewModels.Checkout
{
    public class CheckoutViewModel
    {
        public CartViewModel? Cart { get; set; }
        public ShippingAddressViewModel? ShippingAddress { get; set; }
        public string? AppliedCoupon { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? DiscountType { get; set; } // "percent" or "fixed"
        public string? PaymentMethod { get; set; }
    }
}
