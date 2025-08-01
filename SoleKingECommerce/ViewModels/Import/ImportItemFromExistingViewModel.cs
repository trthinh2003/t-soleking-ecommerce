using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Import
{
    public class ImportItemFromExistingViewModel
    {
        public int ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá nhập không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn 0")]
        public decimal ImportPrice { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        public bool IsSelected { get; set; }
    }
}
