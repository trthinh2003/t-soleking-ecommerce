using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Services.Interfaces;

namespace SoleKingECommerce.Controllers.Customer
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync();
            return View("~/Views/Customer/Cart/Index.cshtml", cart);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItemByVariant(int productId, int variantId, int quantity)
        {
            try
            {
                var cart = await _cartService.UpdateCartItemByVariantAsync(productId, variantId, quantity);
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);

                return Json(new
                {
                    success = true,
                    itemPrice = item?.Price ?? 0,
                    totalItems = cart.TotalItems,
                    totalPrice = cart.TotalPrice
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCartItemByVariant(int productId, int variantId)
        {
            try
            {
                var cart = await _cartService.RemoveCartItemByVariantAsync(productId, variantId);
                return Json(new
                {
                    success = true,
                    totalItems = cart.TotalItems,
                    totalPrice = cart.TotalPrice
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    success = false,
                    requiresLogin = true,
                    message = "Vui lòng đăng nhập để sử dụng mã giảm giá"
                });
            }

            // Giả lập kiểm tra mã giảm giá
            var validCoupons = new Dictionary<string, decimal>
            {
                { "WELCOME10", 0.1m }, // Giảm 10%
                { "SAVE50K", 50000 }, // Giảm 50,000đ
                { "FREESHIP", 0 } // Miễn phí vận chuyển
            };

            if (validCoupons.TryGetValue(couponCode.ToUpper(), out var discountValue))
            {
                return Json(new
                {
                    success = true,
                    couponCode = couponCode.ToUpper(),
                    discountValue = discountValue,
                    discountType = discountValue < 1 ? "percent" : "fixed"
                });
            }

            return Json(new { success = false, message = "Mã giảm giá không hợp lệ" });
        }

        [HttpPost]
        public async Task<IActionResult> ProceedToCheckout()
        {
            var cart = await _cartService.GetCartAsync();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", "Checkout");
        }
    }
}