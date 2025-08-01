using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Category;

namespace SoleKingECommerce.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]/[action]")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string searchString, int? parentId, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentParentId"] = parentId;

            var parentCategories = await _categoryService.GetParentCategoriesForFilterAsync();
            ViewBag.ParentCategories = parentCategories;

            int pageSize = 5;
            int pageNumberInt = pageNumber ?? 1;

            var categories = await _categoryService.GetPaginatedCategoriesAsync(searchString, parentId, pageNumberInt, pageSize);
            return View("~/Views/Admin/Categories/Index.cshtml", categories);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
                CreatedAt = category.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = category.UpdatedAt ?? DateTime.MinValue,
                ProductCount = category.Products?.Count ?? 0
            };

            if (category.ParentId.HasValue)
            {
                var parent = await _categoryService.GetCategoryByIdAsync(category.ParentId.Value);
                ViewBag.ParentCategory = new CategoryViewModel
                {
                    Id = parent.Id,
                    Name = parent.Name
                };
            }

            return View("~/Views/Admin/Categories/Details.cshtml", model);
        }

        public async Task<IActionResult> Create()
        {
            var model = await _categoryService.GetCategoryForCreateAsync();
            return View("~/Views/Admin/Categories/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.CreateCategoryAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var allCategories = await _categoryService.GetAllCategoriesAsync();
            model.ParentCategories = allCategories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentId = c.ParentId,
                CreatedAt = c.CreatedAt ?? DateTime.Now,
                UpdatedAt = c.UpdatedAt ?? DateTime.Now
            }).ToList();
            return View("~/Views/Admin/Categories/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _categoryService.GetCategoryForEditAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Categories/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateCategoryAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var allCategories = await _categoryService.GetAllCategoriesAsync();
            model.ParentCategories = allCategories
                .Where(c => c.Id != id)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    CreatedAt = c.CreatedAt ?? DateTime.Now,
                    UpdatedAt = c.UpdatedAt ?? DateTime.Now
                }).ToList();

            return View("~/Views/Admin/Categories/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Xóa danh mục thành công!" });
                }

                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục" });
                }
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
