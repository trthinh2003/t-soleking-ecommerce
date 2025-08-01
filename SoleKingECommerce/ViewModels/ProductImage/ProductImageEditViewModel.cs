namespace SoleKingECommerce.ViewModels.ProductImage
{
    public class ProductImageEditViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; } // "Ảnh mặt trước", "Ảnh mặt sau", "Ảnh thêm",...
        public bool IsDeleted { get; set; }
    }
}
