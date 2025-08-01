using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Product;

namespace SoleKingECommerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;
    private readonly ICartService _cartService;
    private readonly ICategoryService _categoryService;

    public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService, ICategoryService categoryService)
    {
        _logger = logger;
        _productService = productService;
        _cartService = cartService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var featuredProducts = await _productService.GetPaginatedProductsAsync(
                searchString: null,
                categoryId: null,
                priceRange: null,
                pageNumber: 1,
                pageSize: 8
            );

            return View("~/Views/Customer/Home/Index.cshtml", featuredProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            return View("~/Views/Customer/Home/Index.cshtml", new List<ProductListViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cartService.GetCartAsync();
        _logger.LogInformation($"Cart items count: {cart.Items.Count}");
        _logger.LogInformation($"Total items: {cart.TotalItems}");
        return PartialView("_CartPartial", cart);
    }

    [HttpGet]
    public async Task<IActionResult> GetCartItemCount()
    {
        var count = _cartService.GetCartItemCount();
        return Json(count);
    }

    public IActionResult About()
    {
        return View("~/Views/Customer/Home/About.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        try
        {
            var cart = await _cartService.AddItemToCartAsync(productId, quantity);
            return Json(new { success = true, itemCount = cart.TotalItems });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(int productId, int quantity)
    {
        try
        {
            var cart = await _cartService.UpdateCartItemAsync(productId, quantity);
            return Json(new { success = true, itemCount = cart.TotalItems });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        try
        {
            var cart = await _cartService.RemoveItemFromCartAsync(productId);
            return Json(new { success = true, itemCount = cart.TotalItems });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}