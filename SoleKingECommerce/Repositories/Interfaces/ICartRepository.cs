using SoleKingECommerce.Models;
using SoleKingECommerce.ViewModels.Cart;

namespace SoleKingECommerce.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<CartViewModel> GetCartAsync(string cartId);
        Task<CartViewModel> AddItemToCartAsync(string cartId, int productId, int quantity);
        Task<CartViewModel> UpdateCartItemAsync(string cartId, int productId, int quantity);
        Task<CartViewModel> RemoveItemFromCartAsync(string cartId, int productId);
        Task ClearCartAsync(string cartId);


        //Dành cho merge cart
        Task<Cart?> GetCartByUserIdAsync(string userId);
        Task<Cart> CreateCartAsync(string userId);
        Task<Cart> UpdateCartAsync(Cart cart);
        Task DeleteCartAsync(string userId);
        Task<CartViewModel> GetCartViewModelByUserIdAsync(string userId);
        Task SaveCartItemsAsync(string userId, List<CartItemViewModel> items);
    }
}
