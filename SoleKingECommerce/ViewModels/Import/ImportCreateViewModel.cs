using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Import
{
    public class ImportCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        public int SupplierId { get; set; }

        public string? Note { get; set; }

        public List<ImportItemViewModel> Items { get; set; } = new List<ImportItemViewModel>();
    }
}
