using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Import;

namespace SoleKingECommerce.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Index(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSupplierId"] = supplierId;
            ViewData["CurrentFromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentToDate"] = toDate?.ToString("yyyy-MM-dd");

            ViewBag.Suppliers = await _importService.GetSuppliersAsync();

            int pageSize = 5;
            int pageNumberInt = pageNumber ?? 1;

            var imports = await _importService.GetPaginatedImportsAsync(
                searchString,
                supplierId,
                fromDate,
                toDate,
                pageNumberInt,
                pageSize);

            return View("~/Views/Admin/Imports/Index.cshtml", imports);
        }

        [HttpGet]
        public async Task<IActionResult> AddImportItem(int? index)
        {
            ViewBag.Categories = await _importService.GetLeafCategoriesWithParentNamesAsync();
            ViewBag.Colors = await _importService.GetColorsAsync();
            ViewBag.ItemIndex = index ?? 0;

            var model = new ImportItemViewModel
            {
                Index = index ?? 0
            };
            return PartialView("_ImportItemPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetColorInfo(int colorId)
        {
            try
            {
                var colors = await _importService.GetColorsAsync();
                var color = colors.FirstOrDefault(c => c.Id == colorId);

                if (color != null)
                {
                    return Json(new
                    {
                        id = color.Id,
                        name = color.Name,
                        hex = color.Hex,
                        success = true
                    });
                }

                return Json(new { success = false, message = "Không tìm thấy màu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting color info for colorId: {ColorId}", colorId);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        // GET: Admin/Imports/Details/5
        public async Task<IActionResult> Details(int id)
        {
            ViewBag.Categories = (await _importService.GetLeafCategoriesWithParentNamesAsync());
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
        public async Task<IActionResult> Create([FromForm] ImportCreateViewModel model)
        {
            // Debug logging for images
            _logger.LogInformation("=== CREATE IMPORT DEBUG ===");
            _logger.LogInformation($"Total items received: {model.Items?.Count ?? 0}");

            if (model.Items != null)
            {
                for (int i = 0; i < model.Items.Count; i++)
                {
                    var item = model.Items[i];
                    _logger.LogInformation($"=== Item {i} Debug ===");
                    _logger.LogInformation($"Item {i}: ProductName={item.ProductName}");
                    _logger.LogInformation($"Item {i}: Brand={item.Brand}");
                    _logger.LogInformation($"Item {i}: CategoryId={item.CategoryId}");
                    _logger.LogInformation($"Item {i}: ImportPrice={item.ImportPrice}");
                    _logger.LogInformation($"Item {i}: Images count={item.Images?.Length ?? 0}");

                    // Debug specifications
                    _logger.LogInformation($"Item {i}: Specifications count={item.Specifications?.Count ?? 0}");
                    if (item.Specifications != null)
                    {
                        for (int j = 0; j < item.Specifications.Count; j++)
                        {
                            _logger.LogInformation($"Item {i}, Spec {j}: '{item.Specifications[j]}'");

                            // Parse and validate each specification
                            if (!string.IsNullOrEmpty(item.Specifications[j]))
                            {
                                var parts = item.Specifications[j].Split('-');
                                if (parts.Length >= 3)
                                {
                                    if (int.TryParse(parts[0], out int colorId) &&
                                        int.TryParse(parts[2], out int quantity))
                                    {
                                        _logger.LogInformation($"  Parsed - ColorId: {colorId}, Size: '{parts[1]}', Quantity: {quantity}");
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"  Failed to parse - Parts: [{string.Join(", ", parts)}]");
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning($"  Invalid format - Expected 3 parts, got {parts.Length}");
                                }
                            }
                        }
                    }

                    if (item.Images != null)
                    {
                        for (int j = 0; j < item.Images.Length; j++)
                        {
                            var img = item.Images[j];
                            if (img != null)
                            {
                                _logger.LogInformation($"Item {i}, Image {j}: FileName={img.FileName}, Size={img.Length}, ContentType={img.ContentType}");
                            }
                            else
                            {
                                _logger.LogWarning($"Item {i}, Image {j}: NULL");
                            }
                        }
                    }
                }
            }

            // Log form data for debugging
            _logger.LogInformation("Form data keys:");
            foreach (var key in Request.Form.Keys)
            {
                if (key.Contains("Images"))
                {
                    _logger.LogInformation($"Form key: {key}");
                }
            }

            _logger.LogInformation("Form files:");
            foreach (var file in Request.Form.Files)
            {
                _logger.LogInformation($"File: Name={file.Name}, FileName={file.FileName}, Size={file.Length}");
            }

            // Validate model - Cập nhật logic validation
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("Items", "Vui lòng thêm ít nhất một sản phẩm");
            }
            else
            {
                for (int i = 0; i < model.Items.Count; i++)
                {
                    var item = model.Items[i];
                    if (item.Specifications == null || !item.Specifications.Any() ||
                        item.Specifications.All(s => string.IsNullOrEmpty(s)))
                    {
                        ModelState.AddModelError($"Items[{i}].Specifications",
                            $"Sản phẩm #{i + 1}: Vui lòng thêm ít nhất một thông số kỹ thuật (màu, size, số lượng)");
                    }
                    else
                    {
                        bool hasValidSpec = false;
                        foreach (var spec in item.Specifications)
                        {
                            if (!string.IsNullOrEmpty(spec))
                            {
                                var parts = spec.Split('-');
                                if (parts.Length >= 3 &&
                                    int.TryParse(parts[0], out int colorId) &&
                                    !string.IsNullOrEmpty(parts[1]) &&
                                    int.TryParse(parts[2], out int quantity) &&
                                    colorId > 0 && quantity > 0)
                                {
                                    hasValidSpec = true;
                                    break;
                                }
                            }
                        }

                        if (!hasValidSpec)
                        {
                            ModelState.AddModelError($"Items[{i}].Specifications",
                                $"Sản phẩm #{i + 1}: Thông số kỹ thuật không hợp lệ");
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    _logger.LogWarning("Validation error for {Key}: {Errors}",
                        error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                }
                ViewBag.Suppliers = await _importService.GetSuppliersAsync();
                ViewBag.Categories = await _importService.GetCategoriesAsync();
                ViewBag.Colors = await _importService.GetColorsAsync();
                return View("~/Views/Admin/Imports/Create.cshtml", model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Creating import for user: {user?.Id}");
                await _importService.CreateImportAsync(model, user!.Id);
                TempData["SuccessMessage"] = "Nhập hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating import");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                ViewBag.Suppliers = await _importService.GetSuppliersAsync();
                ViewBag.Categories = await _importService.GetCategoriesAsync();
                ViewBag.Colors = await _importService.GetColorsAsync();
                return View("~/Views/Admin/Imports/Create.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromExisting(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                await _importService.CreateImportFromExistingAsync(id, user!.Id);

                TempData["SuccessMessage"] = "Tạo phiếu nhập mới từ phiếu cũ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phiếu nhập từ phiếu cũ");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }


        [HttpGet]
        public async Task<IActionResult> CreateFromImport(int id)
        {
            ViewBag.Suppliers = await _importService.GetSuppliersAsync();
            ViewBag.Categories = await _importService.GetLeafCategoriesWithParentNamesAsync();
            ViewBag.Colors = await _importService.GetColorsAsync();

            var import = await _importService.GetImportDetailsAsync(id);
            if (import == null) return NotFound();

            var colors = await _importService.GetColorsAsync();

            // Log để debug
            _logger.LogInformation($"=== DEBUG CreateFromImport for Import #{id} ===");
            _logger.LogInformation($"Total items in original import: {import.Items?.Count ?? 0}");

            // Nhóm các items theo sản phẩm (ProductName + Brand + CategoryId)
            var groupedItems = import.Items
                .GroupBy(item => new {
                    item.ProductName,
                    item.Brand,
                    item.CategoryId
                })
                .Select((group, index) =>
                {
                    var firstItem = group.First();

                    // Log thông tin group
                    _logger.LogInformation($"Processing group {index}: {firstItem.ProductName}");
                    _logger.LogInformation($"Items in group: {group.Count()}");

                    var specifications = group.Select((item, itemIndex) =>
                    {
                        // Log thông tin từng item trong group
                        _logger.LogInformation($"  Item {itemIndex}: ColorName='{item.ColorName}', Size='{item.Size}', Quantity={item.Quantity}");

                        // Tìm màu với nhiều cách khác nhau
                        var itemColor = colors.FirstOrDefault(c =>
                            string.Equals(c.Name, item.ColorName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(c.Name.Trim(), item.ColorName?.Trim(), StringComparison.OrdinalIgnoreCase)
                        );

                        var itemColorId = itemColor?.Id ?? 0;

                        // Log kết quả tìm màu
                        if (itemColor == null)
                        {
                            _logger.LogWarning($"  Could not find color '{item.ColorName}' in colors list");
                            _logger.LogInformation($"  Available colors: {string.Join(", ", colors.Select(c => $"'{c.Name}'"))}");
                        }
                        else
                        {
                            _logger.LogInformation($"  Found color '{item.ColorName}' with ID {itemColorId}");
                        }

                        var specValue = $"{itemColorId}-{item.Size}-{item.Quantity}";
                        _logger.LogInformation($"  Generated spec: {specValue}");

                        return specValue;
                    }).ToList();

                    _logger.LogInformation($"Final specifications for {firstItem.ProductName}: [{string.Join(", ", specifications)}]");

                    return new ImportItemViewModel
                    {
                        Index = index,
                        ProductName = firstItem.ProductName,
                        Brand = firstItem.Brand,
                        CategoryId = firstItem.CategoryId,
                        ImportPrice = firstItem.Price,
                        SellPrice = firstItem.Price,
                        Description = "",
                        Specifications = specifications,
                        PreviewImageUrls = new List<string> { firstItem.ImageUrl }
                    };
                })
                .ToList();

            var model = new ImportCreateViewModel
            {
                SupplierId = import.SupplierId,
                Note = $"Nhập thêm từ phiếu #{import.Id}",
                Items = groupedItems
            };

            _logger.LogInformation($"=== Final Model Summary ===");
            _logger.LogInformation($"Total grouped items: {groupedItems.Count}");
            foreach (var item in groupedItems)
            {
                _logger.LogInformation($"Product: {item.ProductName}, Specs count: {item.Specifications?.Count ?? 0}");
                if (item.Specifications != null)
                {
                    for (int i = 0; i < item.Specifications.Count; i++)
                    {
                        _logger.LogInformation($"  Spec {i}: {item.Specifications[i]}");
                    }
                }
            }

            return View("~/Views/Admin/Imports/Create.cshtml", model);
        }

        // GET: Admin/Imports/History
        public async Task<IActionResult> History(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSupplierId"] = supplierId;
            ViewData["CurrentFromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentToDate"] = toDate?.ToString("yyyy-MM-dd");

            ViewBag.Suppliers = await _importService.GetSuppliersAsync();

            int pageSize = 10;
            int pageNumberInt = pageNumber ?? 1;

            var history = await _importService.GetImportHistoryAsync(
                searchString,
                supplierId,
                fromDate,
                toDate,
                pageNumberInt,
                pageSize);

            return View("~/Views/Admin/Imports/History.cshtml", history);
        }

        // GET: Admin/Imports/HistoryDetails/5
        public async Task<IActionResult> HistoryDetails(int id)
        {
            var historyDetail = await _importService.GetImportHistoryDetailAsync(id);
            if (historyDetail == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Imports/HistoryDetails.cshtml", historyDetail);
        }

        // POST: Admin/Imports/ExportHistoryExcel
        [HttpPost]
        public async Task<IActionResult> ExportHistoryExcel(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var data = await _importService.GetImportHistoryForExportAsync(searchString, supplierId, fromDate, toDate);
                var excelFile = await _importService.ExportHistoryToExcelAsync(data);

                var fileName = $"LichSuNhapHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting import history to Excel");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất file Excel";
                return RedirectToAction(nameof(History));
            }
        }

        // POST: Admin/Imports/ExportHistoryPdf
        [HttpPost]
        public async Task<IActionResult> ExportHistoryPdf(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                _logger.LogInformation("Starting PDF export process...");

                var data = await _importService.GetImportHistoryForExportAsync(searchString, supplierId, fromDate, toDate);
                _logger.LogInformation($"Retrieved {data.Count()} records for PDF export");

                var pdfFile = await _importService.ExportHistoryToPdfAsync(data);
                _logger.LogInformation($"PDF generated successfully, size: {pdfFile.Length} bytes");

                var fileName = $"LichSuNhapHang_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfFile, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting import history to PDF");

                // Log inner exception if exists
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: " + ex.InnerException.Message);
                }

                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xuất file PDF: {ex.Message}";
                return RedirectToAction(nameof(History));
            }
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