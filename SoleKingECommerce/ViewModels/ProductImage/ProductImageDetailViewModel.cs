namespace SoleKingECommerce.ViewModels.ProductImage
{
    public class ProductImageDetailViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public string ImageType { get; set; } = "Additional"; 
    }
}
