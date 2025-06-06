using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.ViewModels.Import;
using System.Security.Claims;
using SoleKingECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using SoleKingECommerce.Models;
using System.Text.Json;

namespace SoleKingECommerce.Controllers.Admin
{
    [Route("admin/[controller]/[action]")]
    public class ImportsController : Controller
    {
        private readonly IImportService _importService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ImportsController> _logger;

        public ImportsController(IImportService importService, UserManager<User> userManager, ILogger<ImportsController> logger)
        {
            _importService = importService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Admin/Imports/Index
        public async Task<IActionResult> Index(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSupplierId"] = supplierId;
            ViewData["CurrentFromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentToDate"] = toDate?.ToString("yyyy-MM-dd");

            ViewBag.Suppliers = await _importService.GetSuppliersAsync();

            IEnumerable<ImportListViewModel> imports;
            if (!string.IsNullOrEmpty(searchString) || supplierId.HasValue || fromDate.HasValue || toDate.HasValue)
            {
                imports = await _importService.SearchImportsAsync(searchString, supplierId, fromDate, toDate);
            }
            else
            {
                imports = await _importService.GetAllImportsAsync();
            }

            return View("~/Views/Admin/Imports/Index.cshtml", imports);
        }

        [HttpGet]
        public async Task<IActionResult> AddImportItem(int? index)
        {
            ViewBag.Categories = await _importService.GetCategoriesAsync();
            ViewBag.Colors = await _importService.GetColorsAsync();
            ViewBag.ItemIndex = index ?? 0;

            var model = new ImportItemViewModel();
            return PartialView("_ImportItemPartial", model);
        }

        // GET: Admin/Imports/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var import = await _importService.GetImportDetailsAsync(id);
            if (import == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Imports/Details.cshtml", import);
        }

        // GET: Admin/Imports/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = await _importService.GetSuppliersAsync();
            ViewBag.Categories = await _importService.GetCategoriesAsync();
            ViewBag.Colors = await _importService.GetColorsAsync();

            var model = new ImportCreateViewModel
            {
                Items = new List<ImportItemViewModel>()
            };
            return View("~/Views/Admin/Imports/Create.cshtml", model);
        }

        // POST: Admin/Imports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImportCreateViewModel model)
        {
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            _logger.LogInformation($"Items count: {model.Items?.Count ?? 0}");

            // Log form data để debug
            var requestForm = HttpContext.Request.Form;
            foreach (var key in requestForm.Keys)
            {
                if (key.StartsWith("Items["))
                {
                    _logger.LogInformation($"Form key: {key}, Value: {requestForm[key]}");
                }
            }

            // Validate có ít nhất 1 sản phẩm
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("Items", "Vui lòng thêm ít nhất một sản phẩm");
            }
            else
            {
                // Validate từng item
                for (int i = 0; i < model.Items.Count; i++)
                {
                    var item = model.Items[i];
                    if (string.IsNullOrEmpty(item.ProductName))
                    {
                        ModelState.AddModelError($"Items[{i}].ProductName", "Tên sản phẩm không được để trống");
                    }
                    if (item.Quantity <= 0)
                    {
                        ModelState.AddModelError($"Items[{i}].Quantity", "Số lượng phải lớn hơn 0");
                    }
                    if (item.ImportPrice <= 0)
                    {
                        ModelState.AddModelError($"Items[{i}].ImportPrice", "Giá nhập phải lớn hơn 0");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        _logger.LogError($"ModelState Error - Key: {modelError.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    await _importService.CreateImportAsync(model, user!.Id);

                    TempData["SuccessMessage"] = "Nhập hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating import");
                    TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                }
            }

            // Reload data for dropdown
            ViewBag.Suppliers = await _importService.GetSuppliersAsync();
            ViewBag.Categories = await _importService.GetCategoriesAsync();
            ViewBag.Colors = await _importService.GetColorsAsync();

            return View("~/Views/Admin/Imports/Create.cshtml", model);
        }

        // POST: Admin/Imports/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _importService.DeleteImportAsync(id);
                return Json(new { success = true, message = "Xóa phiếu nhập hàng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}