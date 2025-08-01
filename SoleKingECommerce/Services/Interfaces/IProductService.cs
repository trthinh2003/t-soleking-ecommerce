using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.ViewModels.Product;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface IProductService
    {
        Task<PaginatedList<ProductListViewModel>> GetPaginatedProductsAsync(
            string? searchString,
            int? categoryId,
            string? priceRange,
            int pageNumber,
            int pageSize);

        Task<Product?> GetProductByIdAsync(int id);
        Task<ProductCreateViewModel> GetProductForCreateAsync();
        Task<ProductEditViewModel?> GetProductForEditAsync(int id);
        Task CreateProductAsync(ProductCreateViewModel model);
        Task UpdateProductAsync(ProductEditViewModel model);
        Task UpdateVariantStockAsync(int variantId, int quantity);
        Task DeleteProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
    }
}
