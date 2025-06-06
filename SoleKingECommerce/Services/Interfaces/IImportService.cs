using SoleKingECommerce.Models;
using SoleKingECommerce.ViewModels.Import;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface IImportService
    {
        Task<IEnumerable<ImportListViewModel>> GetAllImportsAsync();
        Task<ImportDetailsViewModel?> GetImportDetailsAsync(int id);
        Task<Import> CreateImportAsync(ImportCreateViewModel model, string userId);
        Task DeleteImportAsync(int id);
        Task<IEnumerable<ImportListViewModel>> SearchImportsAsync(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Color>> GetColorsAsync();
        Task<IEnumerable<Supplier>> GetSuppliersAsync();
    }
}
