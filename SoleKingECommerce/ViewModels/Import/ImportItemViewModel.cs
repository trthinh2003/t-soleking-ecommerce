using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Import
{
    public class ImportItemViewModel
    {
        public int Index { get; set; }
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thương hiệu không được để trống")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn màu")]
        public int ColorId { get; set; }

        [Required(ErrorMessage = "Size không được để trống")]
        public string Size { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Giá nhập không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn 0")]
        public decimal ImportPrice { get; set; }

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
        public decimal SellPrice { get; set; }

        public string? Description { get; set; }
    }
}
