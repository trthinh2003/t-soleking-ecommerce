using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.ViewModels.Product;
using SoleKingECommerce.ViewModels.ProductImage;
using SoleKingECommerce.ViewModels.ProductVariant;

namespace SoleKingECommerce.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByNameAndBrandAsync(string name, string brand);
        Task<PaginatedList<ProductListViewModel>> GetPaginatedProductsAsync(
            string? searchString,
            int? categoryId,
            string? priceRange,
            int pageNumber,
            int pageSize);

        Task<Product?> GetProductByIdWithDetailsAsync(int id);
        Task<ProductEditViewModel?> GetProductForEditAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<ProductVariant?> GetVariantAsync(int productId, int colorId, string size);
        Task<ProductVariant> CreateVariantAsync(ProductVariant variant);
        Task<Product> UpdateAsync(Product product);
        Task<ProductVariant> UpdateVariantAsync(ProductVariant variant);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Color>> GetColorsAsync();
        Task<IEnumerable<Supplier>> GetSuppliersAsync();
        Task DeleteAsync(int id);
        Task<bool> ProductExistsAsync(int id);

        // Cho bên Sale
        Task<List<ProductVariant>> GetProductVariantsAsync(int productId);
        Task<List<ProductImage>> GetProductImagesAsync(int productId);
        Task UpdateProductVariantsAsync(int productId, List<ProductVariantEditViewModel> variants);
        Task UpdateProductImagesAsync(int productId, List<ProductImageEditViewModel> images);
        Task DeleteProductVariantsAsync(List<int> variantIds);
        Task DeleteProductImagesAsync(List<int> imageIds);
    }
}
