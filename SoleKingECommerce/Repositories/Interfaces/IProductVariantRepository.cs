using SoleKingECommerce.Models;

namespace SoleKingECommerce.Repositories.Interfaces
{
    public interface IProductVariantRepository
    {
        Task<ProductVariant?> GetByProductColorSizeAsync(int productId, int colorId, string size);
        Task<ProductVariant> CreateAsync(ProductVariant variant);
        Task<ProductVariant> UpdateAsync(ProductVariant variant);
        Task<IEnumerable<ProductVariant>> GetByProductAsync(int productId);
    }
}
