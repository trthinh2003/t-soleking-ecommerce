using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.ViewModels.Cart;

namespace SoleKingECommerce.Repositories.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly SoleKingDbContext _context;

        public CartRepository(SoleKingDbContext context)
        {
            _context = context;
        }

        public async Task<CartViewModel> GetCartAsync(string cartId)
        {
            return new CartViewModel();
        }

        public async Task<Cart?> GetCartByUserIdAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Color)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart> CreateCartAsync(string userId)
        {
            var cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>()
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<Cart> UpdateCartAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task DeleteCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CartViewModel> GetCartViewModelByUserIdAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return new CartViewModel();
            }

            var cartViewModel = new CartViewModel
            {
                Items = cart.Items.Select(ci => new CartItemViewModel
                {
                    ProductId = ci.ProductVariant.ProductId,
                    ProductName = ci.ProductVariant.Product?.Name ?? "N/A",
                    ImageUrl = ci.ProductVariant.Product?.ImageUrl ?? "",
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    VariantId = ci.ProductVariantId,
                    Size = ci.ProductVariant.Size ?? "N/A",
                    ColorName = ci.ProductVariant.Color?.Name ?? "N/A",
                    SKU = ci.ProductVariant.SKU ?? "N/A",
                    AvailableStock = ci.ProductVariant.Stock
                }).ToList()
            };

            cartViewModel.TotalItems = cartViewModel.Items.Sum(i => i.Quantity);
            cartViewModel.TotalPrice = cartViewModel.Items.Sum(i => i.Price * i.Quantity);

            return cartViewModel;
        }

        public async Task SaveCartItemsAsync(string userId, List<CartItemViewModel> items)
        {
            // Xóa cart cũ
            await DeleteCartAsync(userId);

            if (items.Any())
            {
                // Tạo cart mới
                var cart = await CreateCartAsync(userId);

                // Thêm items
                foreach (var item in items)
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductVariantId = item.VariantId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };

                    _context.Set<CartItem>().Add(cartItem);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<CartViewModel> AddItemToCartAsync(string cartId, int productId, int quantity)
        {
            return new CartViewModel();
        }

        public Task<CartViewModel> UpdateCartItemAsync(string cartId, int productId, int quantity)
        {
            throw new NotImplementedException();
        }

        public Task<CartViewModel> RemoveItemFromCartAsync(string cartId, int productId)
        {
            throw new NotImplementedException();
        }

        public Task ClearCartAsync(string cartId)
        {
            throw new NotImplementedException();
        }
    }
}