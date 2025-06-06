using SoleKingECommerce.Models;

namespace SoleKingECommerce.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllWithProductsAsync();
        Task<Category> GetByIdWithProductsAsync(int id);
        IQueryable<Category> GetQueryableWithProducts();
        Task<IEnumerable<Category>> GetParentCategoriesAsync(int? excludeId = null);
        Task<bool> HasProductsAsync(int categoryId);
    }
}
