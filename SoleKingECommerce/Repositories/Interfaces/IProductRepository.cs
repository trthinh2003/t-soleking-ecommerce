using SoleKingECommerce.Models;

namespace SoleKingECommerce.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByNameAndBrandAsync(string name, string brand);
        Task<Product> CreateAsync(Product product);
        Task<ProductVariant?> GetVariantAsync(int productId, int colorId, string size);
        Task<ProductVariant> CreateVariantAsync(ProductVariant variant);
        Task<ProductVariant> UpdateVariantAsync(ProductVariant variant);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Color>> GetColorsAsync();
        Task<IEnumerable<Supplier>> GetSuppliersAsync();
    }
}
