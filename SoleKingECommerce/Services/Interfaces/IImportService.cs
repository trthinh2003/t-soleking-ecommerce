using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.ViewModels.Import;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface IImportService
    {
        Task<IEnumerable<ImportListViewModel>> GetAllImportsAsync();
        Task<PaginatedList<ImportListViewModel>> GetPaginatedImportsAsync(string searchString, int? supplierId, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
        Task<ImportDetailsViewModel?> GetImportDetailsAsync(int id);
        Task<Import> CreateImportAsync(ImportCreateViewModel model, string userId);
        Task CreateImportFromExistingAsync(int originalImportId, string userId);
        Task DeleteImportAsync(int id);
        Task<IEnumerable<ImportListViewModel>> SearchImportsAsync(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<object>> GetLeafCategoriesWithParentNamesAsync();
        Task<IEnumerable<Color>> GetColorsAsync();
        Task<IEnumerable<Supplier>> GetSuppliersAsync();

        //Xử lý việc xem lại lịch sử nhập hàng
        Task<PaginatedList<ImportHistoryViewModel>> GetImportHistoryAsync(
        string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate, int page, int pageSize);

        Task<ImportHistoryDetailViewModel?> GetImportHistoryDetailAsync(int importId);

        Task<IEnumerable<ImportHistoryExportViewModel>> GetImportHistoryForExportAsync(
            string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate);

        Task<byte[]> ExportHistoryToExcelAsync(IEnumerable<ImportHistoryExportViewModel> data);

        Task<byte[]> ExportHistoryToPdfAsync(IEnumerable<ImportHistoryExportViewModel> data);
    }
}
