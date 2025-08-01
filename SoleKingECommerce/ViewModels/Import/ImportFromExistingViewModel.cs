using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Import
{
    public class ImportFromExistingViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn phiếu nhập nguồn")]
        public int SourceImportId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        public int SupplierId { get; set; }

        public string? Note { get; set; }

        public List<ImportItemFromExistingViewModel> Items { get; set; } = new List<ImportItemFromExistingViewModel>();
    }
}
