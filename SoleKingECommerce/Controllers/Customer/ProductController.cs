using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Services.Interfaces;

namespace SoleKingECommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public ProductController(ILogger<ProductController> logger, IProductService productService, ICartService cartService)
        {
            _logger = logger;
            _productService = productService;
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            return View("~/Views/Customer/Product/Index.cshtml");
        }

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                return View("~/Views/Customer/Product/Detail.cshtml", product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product detail");
                return View("~/Views/Customer/Product/Detail.cshtml", null);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCartWithVariant(int productId, int variantId, int quantity = 1)
        {
            try
            {
                var cart = await _cartService.AddItemToCartWithVariantAsync(productId, variantId, quantity);
                return Json(new { success = true, itemCount = cart.TotalItems });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}