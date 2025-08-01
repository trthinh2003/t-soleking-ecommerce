using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Product
{
    public class ProductCreateViewModel
    {
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

        public IFormFile? MainImage { get; set; }
        public List<IFormFile>? AdditionalImages { get; set; }

        // For dropdown lists
        public IEnumerable<object>? Categories { get; set; }
    }
}
