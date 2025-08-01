using SoleKingECommerce.Helpers;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Cart;
using Microsoft.AspNetCore.Identity;
using SoleKingECommerce.Models;

namespace SoleKingECommerce.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly ICartRepository _cartRepository;
        private readonly IColorRepository _colorRepository;
        private readonly UserManager<User> _userManager;
        private const string CartSessionKey = "Cart";
        private readonly ILogger<CartService> _logger;

        public CartService(
            IHttpContextAccessor httpContextAccessor,
            IProductService productService,
            ICartRepository cartRepository,
            IColorRepository colorRepository,
            UserManager<User> userManager,
            ILogger<CartService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
            _cartRepository = cartRepository;
            _colorRepository = colorRepository;
            _userManager = userManager;
            _logger = logger;
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session;
        private bool IsUserAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(_httpContextAccessor.HttpContext?.User);
        }

        public async Task<CartViewModel> GetCartAsync()
        {
            try
            {
                if (IsUserAuthenticated)
                {
                    var userId = GetCurrentUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var cart = await GetDatabaseCartAsync(userId);
                        return cart ?? CreateEmptyCart();
                    }
                }

                // Fallback to session cart
                if (Session != null)
                {
                    var sessionCart = Session.GetObject<CartViewModel>(CartSessionKey);
                    return sessionCart ?? CreateEmptyCart();
                }

                return CreateEmptyCart();
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while getting the cart.");
                return CreateEmptyCart();
            }
        }

        private CartViewModel CreateEmptyCart()
        {
            return new CartViewModel
            {
                Items = new List<CartItemViewModel>(),
                TotalItems = 0,
                TotalPrice = 0
            };
        }

        public async Task<CartViewModel> GetDatabaseCartAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return CreateEmptyCart();
                }

                var cart = await _cartRepository.GetCartViewModelByUserIdAsync(userId);

                // Đảm bảo cart không null và có Items list
                if (cart == null)
                {
                    cart = CreateEmptyCart();
                }
                else if (cart.Items == null)
                {
                    cart.Items = new List<CartItemViewModel>();
                    cart.TotalItems = 0;
                    cart.TotalPrice = 0;
                }

                return cart;
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while getting the database cart.");
                return CreateEmptyCart();
            }
        }

        public async Task MergeSessionCartToDatabaseAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || Session == null)
                {
                    return;
                }

                // Lấy cart từ session
                var sessionCart = Session.GetObject<CartViewModel>(CartSessionKey);
                if (sessionCart == null || sessionCart.Items == null || !sessionCart.Items.Any())
                {
                    return;
                }

                // Lấy cart từ database
                var databaseCart = await GetDatabaseCartAsync(userId);
                var mergedItems = new List<CartItemViewModel>();

                if (databaseCart.Items != null)
                {
                    mergedItems.AddRange(databaseCart.Items);
                }

                foreach (var sessionItem in sessionCart.Items)
                {
                    var existingItem = mergedItems.FirstOrDefault(i =>
                        i.ProductId == sessionItem.ProductId &&
                        i.VariantId == sessionItem.VariantId);

                    if (existingItem != null)
                    {
                        // Merge quantities, nhưng không vượt quá stock
                        var totalQuantity = existingItem.Quantity + sessionItem.Quantity;
                        existingItem.Quantity = Math.Min(totalQuantity, sessionItem.AvailableStock);
                    }
                    else
                    {
                        mergedItems.Add(sessionItem);
                    }
                }

                // Save merged cart to database
                var mergedCart = new CartViewModel
                {
                    Items = mergedItems,
                    TotalItems = mergedItems.Sum(i => i.Quantity),
                    TotalPrice = mergedItems.Sum(i => i.Price * i.Quantity)
                };

                await SaveCartToDatabaseAsync(userId, mergedCart);
                Session.Remove(CartSessionKey);
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while merging session cart to database.");
                throw;
            }
        }

        public async Task SaveCartToDatabaseAsync(string userId, CartViewModel cart)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId) && cart != null && cart.Items != null)
                {
                    await _cartRepository.SaveCartItemsAsync(userId, cart.Items);
                }
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while saving the cart to the database.");
                throw;
            }
        }

        public async Task<CartViewModel> LoadCartFromDatabaseAsync(string userId)
        {
            return await GetDatabaseCartAsync(userId);
        }

        public async Task<CartViewModel> AddItemToCartAsync(int productId, int quantity = 1)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    throw new ArgumentException("Product not found");
                }

                var firstVariant = product.Variants
                    ?.Where(v => v.Stock > 0)
                    ?.OrderBy(v => v.Id)
                    ?.FirstOrDefault();

                if (firstVariant == null)
                {
                    throw new InvalidOperationException("Sản phẩm hiện tại không có biến thể nào còn hàng.");
                }

                if (quantity > firstVariant.Stock)
                {
                    throw new InvalidOperationException($"Biến thể này chỉ còn {firstVariant.Stock} sản phẩm trong kho.");
                }

                var cart = await GetCartAsync();

                var existingItem = cart.Items?.FirstOrDefault(i => i.ProductId == productId && i.VariantId == firstVariant.Id);
                var variantColor = await _colorRepository.GetByIdAsync(firstVariant.ColorId);

                if (existingItem != null)
                {
                    if (existingItem.Quantity + quantity > firstVariant.Stock)
                    {
                        throw new InvalidOperationException($"Không thể thêm sản phẩm nữa. Bạn đã có {existingItem.Quantity} sản phẩm trong giỏ, và biến thể này chỉ còn {firstVariant.Stock} sản phẩm.");
                    }
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Items.Add(new CartItemViewModel
                    {
                        ProductId = productId,
                        ProductName = product.Name ?? "N/A",
                        ImageUrl = product.ImageUrl ?? "",
                        Price = product.BasePrice ?? 0,
                        Quantity = quantity,
                        VariantId = firstVariant.Id,
                        Size = firstVariant.Size ?? "N/A",
                        ColorName = variantColor?.Name ?? "N/A",
                        SKU = firstVariant.SKU ?? "N/A",
                        AvailableStock = firstVariant.Stock
                    });
                }

                cart.TotalItems = cart.Items.Sum(i => i.Quantity);
                cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

                await SaveCart(cart);
                return cart;
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while adding an item to the cart.");
                throw;
            }
        }

        public async Task<CartViewModel> AddItemToCartWithVariantAsync(int productId, int variantId, int quantity = 1)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    throw new ArgumentException("Product not found");
                }

                var variant = product.Variants?.FirstOrDefault(v => v.Id == variantId);
                if (variant == null)
                {
                    throw new ArgumentException("Product variant not found");
                }

                if (quantity > variant.Stock)
                {
                    throw new InvalidOperationException($"Biến thể này chỉ còn {variant.Stock} sản phẩm trong kho.");
                }

                var cart = await GetCartAsync();
                var existingItem = cart.Items?.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);
                var variantColor = await _colorRepository.GetByIdAsync(variant.ColorId);

                if (existingItem != null)
                {
                    if (existingItem.Quantity + quantity > variant.Stock)
                    {
                        throw new InvalidOperationException($"Không thể thêm sản phẩm nữa. Bạn đã có {existingItem.Quantity} sản phẩm trong giỏ, và biến thể này chỉ còn {variant.Stock} sản phẩm.");
                    }
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Items.Add(new CartItemViewModel
                    {
                        ProductId = productId,
                        ProductName = product.Name ?? "N/A",
                        ImageUrl = product.ImageUrl ?? "",
                        Price = variant.Price,
                        Quantity = quantity,
                        VariantId = variant.Id,
                        Size = variant.Size ?? "N/A",
                        ColorName = variantColor?.Name ?? "N/A",
                        SKU = variant.SKU ?? "N/A",
                        AvailableStock = variant.Stock
                    });
                }

                cart.TotalItems = cart.Items.Sum(i => i.Quantity);
                cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

                await SaveCart(cart);
                return cart;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task SaveCart(CartViewModel cart)
        {
            try
            {
                if (IsUserAuthenticated)
                {
                    var userId = GetCurrentUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await SaveCartToDatabaseAsync(userId, cart);
                    }
                }
                else if (Session != null)
                {
                    Session.SetObject(CartSessionKey, cart);
                }
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while saving the cart.");
            }
        }

        public async Task<CartViewModel> UpdateCartItemByVariantAsync(int productId, int variantId, int quantity)
        {
            try
            {
                var cart = await GetCartAsync();
                var item = cart.Items?.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);

                if (item != null)
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    var variant = product?.Variants?.FirstOrDefault(v => v.Id == variantId);

                    if (variant == null)
                    {
                        throw new InvalidOperationException("Biến thể sản phẩm không tồn tại.");
                    }

                    if (quantity > variant.Stock)
                    {
                        throw new InvalidOperationException($"Biến thể này hiện chỉ còn {variant.Stock} sản phẩm trong kho.");
                    }

                    item.Quantity = quantity;

                    if (item.Quantity <= 0)
                    {
                        cart.Items.Remove(item);
                    }

                    cart.TotalItems = cart.Items.Sum(i => i.Quantity);
                    cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

                    await SaveCart(cart);
                }

                return cart;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CartViewModel> RemoveCartItemByVariantAsync(int productId, int variantId)
        {
            try
            {
                var cart = await GetCartAsync();
                var item = cart.Items?.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);

                if (item != null)
                {
                    cart.Items.Remove(item);
                    cart.TotalItems = cart.Items.Sum(i => i.Quantity);
                    cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

                    await SaveCart(cart);
                }

                return cart;
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while removing an item from the cart.");
                throw;
            }
        }

        public async Task<CartViewModel> UpdateCartItemAsync(int productId, int quantity)
        {
            try
            {
                var cart = await GetCartAsync();
                var item = cart.Items?.FirstOrDefault(i => i.ProductId == productId);

                if (item != null)
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    var variant = product?.Variants?.FirstOrDefault(v => v.Id == item.VariantId);

                    if (variant == null)
                    {
                        throw new InvalidOperationException("Biến thể sản phẩm không tồn tại.");
                    }

                    if (quantity > variant.Stock)
                    {
                        throw new InvalidOperationException($"Biến thể này hiện chỉ còn {variant.Stock} sản phẩm trong kho.");
                    }

                    item.Quantity = quantity;

                    if (item.Quantity <= 0)
                    {
                        cart.Items.Remove(item);
                    }

                    cart.TotalItems = cart.Items.Sum(i => i.Quantity);
                    cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

                    await SaveCart(cart);
                }

                return cart;
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while updating the cart item.");
                throw;
            }
        }

        public async Task<CartViewModel> RemoveItemFromCartAsync(int productId)
        {
            try
            {
                var cart = await GetCartAsync();
                var item = cart.Items?.FirstOrDefault(i => i.ProductId == productId);

                if (item != null)
                {
                    cart.Items.Remove(item);
                    cart.TotalItems = cart.Items.Sum(i => i.Quantity);
                    cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

                    await SaveCart(cart);
                }

                return cart;
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while removing an item from the cart.");
                throw;
            }
        }

        public async Task ClearCartAsync()
        {
            try
            {
                if (IsUserAuthenticated)
                {
                    var userId = GetCurrentUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _cartRepository.DeleteCartAsync(userId);
                    }
                }
                else if (Session != null)
                {
                    Session.Remove(CartSessionKey);
                }
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while clearing the cart.");
            }
        }

        public int GetCartItemCount()
        {
            try
            {
                if (IsUserAuthenticated)
                {
                    var userId = GetCurrentUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var cart = GetDatabaseCartAsync(userId).Result;
                        return cart?.TotalItems ?? 0;
                    }
                }
                else if (Session != null)
                {
                    var cart = Session.GetObject<CartViewModel>(CartSessionKey);
                    return cart?.TotalItems ?? 0;
                }

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}