using SoleKingECommerce.ViewModels.Cart;
using System.Threading.Tasks;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync();
        Task<CartViewModel> AddItemToCartAsync(int productId, int quantity = 1);
        Task<CartViewModel> AddItemToCartWithVariantAsync(int productId, int variantId, int quantity = 1);
        Task<CartViewModel> UpdateCartItemAsync(int productId, int quantity);
        Task<CartViewModel> UpdateCartItemByVariantAsync(int productId, int variantId, int quantity);
        Task<CartViewModel> RemoveItemFromCartAsync(int productId);
        Task<CartViewModel> RemoveCartItemByVariantAsync(int productId, int variantId);
        Task ClearCartAsync();
        int GetCartItemCount();

        // Dành cho merge cart
        Task<CartViewModel> GetDatabaseCartAsync(string userId);
        Task MergeSessionCartToDatabaseAsync(string userId);
        Task SaveCartToDatabaseAsync(string userId, CartViewModel cart);
        Task<CartViewModel> LoadCartFromDatabaseAsync(string userId);
    }
}