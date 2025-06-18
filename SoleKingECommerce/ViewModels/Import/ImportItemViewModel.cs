using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Import
{
    public class ImportItemViewModel
    {
        public int Index { get; set; }
        public int? ProductId { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile[] Images { get; set; } = Array.Empty<IFormFile>(); 
        public List<string> PreviewImageUrls { get; set; } = new List<string>();

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thương hiệu không được để trống")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Giá nhập không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn 0")]
        public decimal ImportPrice { get; set; }

        public decimal? SellsPrice { get; set; } = 0;

        public decimal? SellPrice { get; set; } = 0;

        public string? Description { get; set; }

        // Thay đổi từ List<ProductSpecification> thành List<string>
        public List<string> Specifications { get; set; } = new List<string>();

        // Property để parse specifications
        public List<ProductSpecification> ParsedSpecifications
        {
            get
            {
                var result = new List<ProductSpecification>();
                foreach (var spec in Specifications)
                {
                    if (!string.IsNullOrEmpty(spec))
                    {
                        var parts = spec.Split('-');
                        if (parts.Length >= 3)
                        {
                            if (int.TryParse(parts[0], out int colorId) &&
                                int.TryParse(parts[2], out int quantity))
                            {
                                result.Add(new ProductSpecification
                                {
                                    ColorId = colorId,
                                    Size = parts[1],
                                    Quantity = quantity
                                });
                            }
                        }
                    }
                }
                return result;
            }
        }
    }

    public class ProductSpecification
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public string DisplayText => $"Màu: {ColorName} - Size: {Size} - Số lượng: {Quantity}";
    }
}
