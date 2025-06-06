using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.ViewModels.Supplier;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier?> GetSupplierByIdAsync(int id);
        Task<PaginatedList<Supplier>> GetPaginatedSuppliersAsync(string searchString, int pageIndex = 1, int pageSize = 10);
        Task CreateSupplierAsync(SupplierViewModel model);
        Task UpdateSupplierAsync(SupplierViewModel model);
        Task DeleteSupplierAsync(int id);
        Task<bool> SupplierExistsAsync(int id);
    }
}
