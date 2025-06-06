using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Services.Implementations;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Category;
using SoleKingECommerce.ViewModels.Supplier;

namespace SoleKingECommerce.Controllers.Admin
{
    [Route("admin/[controller]/[action]")]
    public class SuppliersController : Controller
    {
        private readonly ILogger<SuppliersController> _logger;
        private readonly ISupplierService _supplierService;
        public SuppliersController(ILogger<SuppliersController> logger, ISupplierService supplierService)
        {
            _logger = logger;
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;
                int pageSize = 5;
                int pageNumberInt = pageNumber ?? 1;
                var suppliers = await _supplierService.GetPaginatedSuppliersAsync(searchString, pageNumberInt, pageSize);
                return View("~/Views/Admin/Suppliers/Index.cshtml", suppliers);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suppliers");
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _supplierService.GetSupplierByIdAsync(id.Value);
            if (supplier == null)
            {
                return NotFound();
            }
            var model = new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Address = supplier.Address,
                Email = supplier.Email,
                Phone = supplier.Phone
            };
            return View("~/Views/Admin/Suppliers/Details.cshtml", model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Suppliers/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _supplierService.CreateSupplierAsync(model);
                    return RedirectToAction(nameof(Index));
                } catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View("~/Views/Admin/Suppliers/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _supplierService.GetSupplierByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }
            var viewModel = new SupplierViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Email = model.Email,
                Phone = model.Phone
            };

            return View("~/Views/Admin/Suppliers/Edit.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _supplierService.UpdateSupplierAsync(model);
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
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            return View("~/Views/Admin/Suppliers/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _supplierService.DeleteSupplierAsync(id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Xóa nhà cung cấp thành cong!" });
                }
            }
            catch (ArgumentException)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không tìm thấy nhà cung cấp" });
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
            return RedirectToAction(nameof(Index));
        }
    }
}
