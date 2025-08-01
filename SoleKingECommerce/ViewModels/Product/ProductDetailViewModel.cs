using SoleKingECommerce.ViewModels.Color;
using SoleKingECommerce.ViewModels.ProductImage;
using SoleKingECommerce.ViewModels.ProductVariant;

namespace SoleKingECommerce.ViewModels.Product
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public string? Tags { get; set; }
        public string? MainImageUrl { get; set; }
        public string? CategoryName { get; set; }


        // Danh sách ảnh sản phẩm (ảnh mặt trước, mặt sau, ảnh phụ)
        public List<ProductImageViewModel> Images { get; set; } = new List<ProductImageViewModel>();
        public List<ProductImageViewModel> FrontImages { get; set; } = new List<ProductImageViewModel>();
        public List<ProductImageViewModel> BackImages { get; set; } = new List<ProductImageViewModel>();
        public List<ProductImageViewModel> AdditionalImages { get; set; } = new List<ProductImageViewModel>();

        // Danh sách variant (màu sắc, kích thước, giá)
        public List<ProductVariantDetailViewModel> Variants { get; set; } = new List<ProductVariantDetailViewModel>();

        // Danh sách màu sắc có sẵn
        public List<ColorViewModel> AvailableColors { get; set; } = new List<ColorViewModel>();

        // Danh sách kích thước có sẵn
        public List<string> AvailableSizes { get; set; } = new List<string>();

        public bool IsInStock { get; set; }
        public decimal TotalStock { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        // Đánh giá
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
