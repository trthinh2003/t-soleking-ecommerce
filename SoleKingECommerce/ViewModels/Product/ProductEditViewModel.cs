using SoleKingECommerce.ViewModels.ProductImage;
using SoleKingECommerce.ViewModels.ProductVariant;
using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Product
{
    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [MaxLength(250, ErrorMessage = "Tên sản phẩm không được vượt quá 250 ký tự")]
        public string? Name { get; set; }

        [MaxLength(100, ErrorMessage = "Thương hiệu không được vượt quá 100 ký tự")]
        public string? Brand { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá gốc phải lớn hơn 0")]
        public decimal? BasePrice { get; set; }

        public string? Tags { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        public string? CurrentImageUrl { get; set; }
        public IFormFile? MainImage { get; set; }
        public List<IFormFile>? AdditionalImages { get; set; }

        public IEnumerable<object>? Categories { get; set; }
        public List<ProductImageViewModel>? ExistingImages { get; set; }

        // Dành cho sales
        public List<ProductVariantEditViewModel>? Variants { get; set; }
        public List<ProductImageEditViewModel>? Images { get; set; }
        public IFormFile? FrontImage { get; set; }
        public IFormFile? BackImage { get; set; }
        public List<int>? DeletedImageIds { get; set; }
        public List<int>? DeletedVariantIds { get; set; }
    }
}
