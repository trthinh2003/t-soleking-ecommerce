using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Cart
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public int TotalItems { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public int VariantId { get; set; }
        public string Size { get; set; }
        public string ColorName { get; set; }
        public string SKU { get; set; }
        public int AvailableStock { get; set; }
    }
}