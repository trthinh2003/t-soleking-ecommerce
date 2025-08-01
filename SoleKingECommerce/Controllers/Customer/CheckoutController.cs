// CheckoutController.cs - Updated
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Cart;
using SoleKingECommerce.ViewModels.Checkout;
using SoleKingECommerce.ViewModels.Payment;
using System.Security.Claims;

namespace SoleKingECommerce.Controllers.Customer
{
    [Authorize(Roles = "Admin,Client")]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IVoucherService _voucherService;
        private readonly IOrderService _orderService;
        private readonly IVnPayService _vnPayService;
        private readonly IProductService _productService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICartService cartService,
            IVoucherService voucherService,
            IOrderService orderService,
            IVnPayService vnPayService,
            IProductService productService,
            ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _voucherService = voucherService;
            _orderService = orderService;
            _vnPayService = vnPayService;
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cart = await _cartService.GetCartAsync();

                if (cart == null)
                {
                    TempData["ErrorMessage"] = "Không thể tải giỏ hàng. Vui lòng thử lại.";
                    return RedirectToAction("Index", "Cart");
                }

                if (cart.Items == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Cart");
                }

                decimal discountAmount = 0;
                if (TempData["DiscountAmount"] is string discountAmountStr)
                {
                    decimal.TryParse(discountAmountStr, out discountAmount);
                }

                var model = new CheckoutViewModel
                {
                    Cart = cart,
                    ShippingAddress = new ShippingAddressViewModel(),
                    AppliedCoupon = TempData["AppliedCoupon"] as string ?? string.Empty,
                    DiscountAmount = discountAmount,
                    DiscountType = TempData["DiscountType"] as string ?? string.Empty
                };

                if (model.Cart == null || model.Cart.Items == null)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin giỏ hàng.";
                    return RedirectToAction("Index", "Cart");
                }

                TempData.Keep("AppliedCoupon");
                TempData.Keep("DiscountAmount");
                TempData.Keep("DiscountType");

                return View("~/Views/Customer/Checkout/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang thanh toán. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để sử dụng mã giảm giá";
                return RedirectToAction("Index");
            }

            try
            {
                var coupon = await _voucherService.GetByCodeAsync(couponCode);
                if (coupon == null || coupon.EndDate < DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn";
                    return RedirectToAction("Index");
                }

                var cart = await _cartService.GetCartAsync();

                if (cart == null || cart.Items == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống, không thể áp dụng mã giảm giá";
                    return RedirectToAction("Index", "Cart");
                }

                if (cart.TotalPrice < coupon.MinOrderAmount)
                {
                    TempData["ErrorMessage"] = $"Đơn hàng tối thiểu {coupon.MinOrderAmount.ToString("N0")}đ để áp dụng mã giảm giá";
                    return RedirectToAction("Index");
                }

                decimal discountAmount = 0;
                string discountType = "percent";

                if (coupon.DiscountPercent > 0)
                {
                    discountAmount = cart.TotalPrice * (coupon.DiscountPercent / 100);
                    if (coupon.MaxDiscount > 0 && discountAmount > coupon.MaxDiscount)
                    {
                        discountAmount = coupon.MaxDiscount;
                    }
                    discountType = "percent";
                }

                TempData["AppliedCoupon"] = couponCode;
                TempData["DiscountAmount"] = discountAmount.ToString();
                TempData["DiscountType"] = discountType;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi áp dụng mã giảm giá";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model, string paymentMethod)
        {
            try
            {
                var cart = await _cartService.GetCartAsync();
                if (cart == null || cart.Items == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống, không thể đặt hàng";
                    return RedirectToAction("Index", "Cart");
                }

                model.Cart = cart;

                if (!ModelState.IsValid)
                {
                    return View("~/Views/Customer/Checkout/Index.cshtml", model);
                }

                // Kiểm tra stock trước khi đặt hàng
                var stockCheckResult = await ValidateStock(cart);
                if (!stockCheckResult.IsValid)
                {
                    TempData["ErrorMessage"] = stockCheckResult.ErrorMessage;
                    return RedirectToAction("Index");
                }

                switch (paymentMethod)
                {
                    case "COD":
                        return await ProcessCodPayment(model, cart);
                    case "VNPAY":
                        return await ProcessVnPayPayment(model, cart);
                    case "MOMO":
                        return await ProcessMoMoPayment(model, cart);
                    case "PAYPAL":
                        return await ProcessPayPalPayment(model, cart);
                    default:
                        ModelState.AddModelError("", "Vui lòng chọn phương thức thanh toán");
                        return View("~/Views/Customer/Checkout/Index.cshtml", model);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    model.Cart = await _cartService.GetCartAsync();
                }
                catch
                {
                    return RedirectToAction("Index", "Cart");
                }

                ModelState.AddModelError("", "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại.");
                return View("~/Views/Customer/Checkout/Index.cshtml", model);
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateStock(CartViewModel cart)
        {
            foreach (var item in cart.Items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var variant = product?.Variants?.FirstOrDefault(v => v.Id == item.VariantId);

                if (variant == null)
                {
                    return (false, $"Sản phẩm {item.ProductName} không tồn tại");
                }

                if (variant.Stock < item.Quantity)
                {
                    return (false, $"Sản phẩm {item.ProductName} chỉ còn {variant.Stock} sản phẩm trong kho");
                }
            }

            return (true, string.Empty);
        }

        private async Task<IActionResult> ProcessCodPayment(CheckoutViewModel model, CartViewModel cart)
        {
            try
            {
                var order = await CreateOrder(model, cart, "COD");

                // Trừ stock và xóa giỏ hàng
                await UpdateStockAndClearCart(cart);

                // Đánh dấu voucher đã sử dụng
                await MarkVoucherAsUsed(model.AppliedCoupon, order.Id);

                TempData["OrderId"] = order.Id;
                return RedirectToAction("OrderConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing COD payment");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xử lý đơn hàng. Vui lòng thử lại.");
                return View("~/Views/Customer/Checkout/Index.cshtml", model);
            }
        }

        private async Task<IActionResult> ProcessVnPayPayment(CheckoutViewModel model, CartViewModel cart)
        {
            try
            {
                // Tạo order tạm thời với status Pending
                var order = await CreateOrder(model, cart, "VNPAY", "Pending");

                // Lưu thông tin order vào session để xử lý sau khi thanh toán
                HttpContext.Session.SetString("PendingOrderId", order.Id.ToString());
                HttpContext.Session.SetString("AppliedCoupon", model.AppliedCoupon ?? "");

                // Tạo request VNPay
                var vnPayRequest = new VnPaymentRequest
                {
                    OrderId = order.Id,
                    FullName = model.ShippingAddress.FullName,
                    Description = $"Thanh toán đơn hàng #{order.Id}",
                    Amount = (double)(cart.TotalPrice + 30000 - model.DiscountAmount),
                    CreatedDate = DateTime.Now
                };

                var paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest, HttpContext);
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay payment");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo thanh toán VNPay.");
                return View("~/Views/Customer/Checkout/Index.cshtml", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);

                if (!response.Success)
                {
                    TempData["ErrorMessage"] = "Thanh toán không thành công";
                    return RedirectToAction("Index", "Cart");
                }

                // Lấy thông tin order từ session
                var pendingOrderIdStr = HttpContext.Session.GetString("PendingOrderId");
                var appliedCoupon = HttpContext.Session.GetString("AppliedCoupon");

                if (string.IsNullOrEmpty(pendingOrderIdStr) || !int.TryParse(pendingOrderIdStr, out int orderId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng";
                    return RedirectToAction("Index", "Cart");
                }

                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index", "Cart");
                }

                // Cập nhật trạng thái order và transaction
                order.Status = "Confirmed";
                order.UpdatedAt = DateTime.Now;

                if (order.Transactions != null && order.Transactions.Any())
                {
                    var transaction = order.Transactions.First();
                    transaction.PaymentStatus = "Completed";
                    transaction.PaidAt = DateTime.Now;
                }

                await _orderService.UpdateOrderAsync(order);

                // Trừ stock và xóa giỏ hàng
                var cart = await _cartService.GetCartAsync();
                await UpdateStockAndClearCart(cart);

                // Đánh dấu voucher đã sử dụng
                await MarkVoucherAsUsed(appliedCoupon, orderId);

                // Xóa session
                HttpContext.Session.Remove("PendingOrderId");
                HttpContext.Session.Remove("AppliedCoupon");

                TempData["OrderId"] = orderId;
                TempData["SuccessMessage"] = "Thanh toán thành công";
                return RedirectToAction("OrderConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay callback");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý kết quả thanh toán";
                return RedirectToAction("Index", "Cart");
            }
        }

        private async Task<Order> CreateOrder(CheckoutViewModel model, CartViewModel cart, string paymentMethod, string status = "Confirmed")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = new Order
            {
                CustomerName = model.ShippingAddress.FullName,
                CustomerPhone = model.ShippingAddress.PhoneNumber,
                CustomerAddress = $"{model.ShippingAddress.StreetAddress}, {model.ShippingAddress.Ward}, {model.ShippingAddress.District}, {model.ShippingAddress.City}",
                TotalPrice = cart.TotalPrice + 30000 - model.DiscountAmount,
                Status = status,
                UserId = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            order.Items = cart.Items.Select(item => new OrderItem
            {
                ProductVariantId = item.VariantId,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList();

            order.Transactions = new List<Transaction>
            {
                new Transaction
                {
                    PaymentMethod = paymentMethod,
                    PaymentStatus = paymentMethod == "COD" ? "Pending" : "Processing",
                    Amount = order.TotalPrice,
                    PaidAt = DateTime.Now
                }
            };

            return await _orderService.CreateOrderAsync(order);
        }

        private async Task UpdateStockAndClearCart(CartViewModel cart)
        {
            // Trừ stock cho từng variant
            foreach (var item in cart.Items)
            {
                await _productService.UpdateVariantStockAsync(item.VariantId, -item.Quantity);
            }

            // Xóa giỏ hàng
            await _cartService.ClearCartAsync();
        }

        private async Task MarkVoucherAsUsed(string couponCode, int orderId)
        {
            if (!string.IsNullOrEmpty(couponCode))
            {
                var voucher = await _voucherService.GetByCodeAsync(couponCode);
                if (voucher != null)
                {
                    await _voucherService.MarkAsUsedAsync(voucher.Id, orderId);
                }
            }
        }

        private async Task<IActionResult> ProcessMoMoPayment(CheckoutViewModel model, CartViewModel cart)
        {
            // Sẽ phát triển sau này :>
            return Redirect("https://payment.momo.vn/");
        }

        private async Task<IActionResult> ProcessPayPalPayment(CheckoutViewModel model, CartViewModel cart)
        {
            // Sẽ phát triển sau này :>
            return Redirect("https://www.paypal.com/checkoutnow");
        }

        public async Task<IActionResult> OrderConfirmation()
        {
            if (TempData["OrderId"] is not int orderId)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View("~/Views/Customer/Checkout/OrderConfirmation.cshtml", order);
        }
    }
}