using SoleKingECommerce.Models;

namespace SoleKingECommerce.Repositories.Interfaces
{
    public interface IImportRepository
    {
        Task<IEnumerable<Import>> GetAllAsync();
        IQueryable<Import> GetQueryable();
        Task<Import?> GetByIdAsync(int id);
        Task<Import> CreateAsync(Import import);
        Task<Import> UpdateAsync(Import import);
        Task DeleteAsync(int id);
        Task<IEnumerable<Import>> GetBySupplierAsync(int supplierId);
        Task<IEnumerable<Import>> SearchAsync(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate);
    }
}
