// Controllers/Admin/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Product;
using SoleKingECommerce.Helpers;

namespace SoleKingECommerce.Controllers.Admin
{
    //[Area("Admin")]
    [Route("admin/[controller]/[action]")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IColorService _colorService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IColorService colorService,
            UserManager<User> userManager,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _colorService = colorService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Admin/Products/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? searchString, int? categoryId, string? priceRange, int? pageNumber)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;
                ViewData["CurrentCategoryId"] = categoryId;
                ViewData["CurrentPriceRange"] = priceRange;

                // Get categories for filter dropdown
                ViewBag.Categories = await _categoryService.GetLeafCategoriesWithParentNamesAsync();

                int pageSize = 10;
                int pageNumberInt = pageNumber ?? 1;

                var products = await _productService.GetPaginatedProductsAsync(
                    searchString,
                    categoryId,
                    priceRange,
                    pageNumberInt,
                    pageSize);

                return View("~/Views/Admin/Products/Index.cshtml", products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading products index");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách sản phẩm.";
                return View("~/Views/Admin/Products/Index.cshtml", new PaginatedList<ProductListViewModel>(new List<ProductListViewModel>(), 0, 1, 10));
            }
        }

        // GET: Admin/Products/Details/5
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Views/Admin/Products/Details.cshtml", product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading product details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin sản phẩm.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Products/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = await _productService.GetProductForCreateAsync();
                return View("~/Views/Admin/Products/Create.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create product form");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải form tạo sản phẩm.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Products/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Categories = await _categoryService.GetLeafCategoriesWithParentNamesAsync();
                    return View("~/Views/Admin/Products/Create.cshtml", model);
                }

                await _productService.CreateProductAsync(model);
                TempData["SuccessMessage"] = "Tạo sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo sản phẩm: " + ex.Message;
                model.Categories = await _categoryService.GetLeafCategoriesWithParentNamesAsync();
                return View("~/Views/Admin/Products/Create.cshtml", model);
            }
        }

        // GET: Admin/Products/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _productService.GetProductForEditAsync(id);
                if (model == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Colors = await _colorService.GetAllColorsAsync();

                return View("~/Views/Admin/Products/Edit.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading edit product form for ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải form chỉnh sửa sản phẩm.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Products/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "ID không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    model.Categories = await _categoryService.GetLeafCategoriesWithParentNamesAsync();
                    return View("~/Views/Admin/Products/Edit.cshtml", model);
                }

                await _productService.UpdateProductAsync(model);
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product with ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật sản phẩm: " + ex.Message;
                model.Categories = await _categoryService.GetLeafCategoriesWithParentNamesAsync();
                return View("~/Views/Admin/Products/Edit.cshtml", model);
            }
        }

        // POST: Admin/Products/Delete
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product with ID: {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa sản phẩm." });
            }
        }

        // Helper method để kiểm tra xem sản phẩm có tồn tại hay không
        private async Task<bool> ProductExists(int id)
        {
            return await _productService.ProductExistsAsync(id);
        }

        [HttpGet("GetColors")]
        public async Task<IActionResult> GetColors()
        {
            try
            {
                var colors = await _colorService.GetAllColorsAsync();
                var result = colors.Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    hex = c.Hex
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading colors");
                return Json(new List<object>());
            }
        }
    }
}