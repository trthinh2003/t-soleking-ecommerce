using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.ViewModels.Category;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<PaginatedList<Category>> GetPaginatedCategoriesAsync(string searchString, int? parentId, int pageIndex = 1, int pageSize = 10);
        Task<IEnumerable<Category>> GetParentCategoriesForFilterAsync();
        Task<IEnumerable<object>> GetLeafCategoriesWithParentNamesAsync();
        Task<CategoryViewModel> GetCategoryForCreateAsync();
        Task<CategoryViewModel> GetCategoryForEditAsync(int id);
        Task CreateCategoryAsync(CategoryViewModel model);
        Task UpdateCategoryAsync(CategoryViewModel model);
        Task DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsAsync(int id);

        // Client
        Task<IEnumerable<CategoryTreeViewModel>> GetCategoryTreeAsync();
        Task<IEnumerable<CategoryTreeViewModel>> GetCachedCategoryTreeAsync();
    }
}
