using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Voucher;

namespace SoleKingECommerce.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]/[action]")]
    public class VouchersController : Controller
    {
        private readonly IVoucherService _voucherService;
        private readonly ILogger<VouchersController> _logger;

        public VouchersController(IVoucherService voucherService, ILogger<VouchersController> logger)
        {
            _voucherService = voucherService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, string searchString = null, string status = null)
        {
            const int pageSize = 10;

            var vouchers = await _voucherService.GetPaginatedVouchersAsync(pageNumber, pageSize, searchString, status);

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;

            return View("~/Views/Admin/Vouchers/Index.cshtml", vouchers);
        }

        public IActionResult Create()
        {
            return View("~/Views/Admin/Vouchers/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVoucherViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu");
                    return View("~/Views/Admin/Vouchers/Create.cshtml", model);
                }

                if (await _voucherService.IsCodeExistsAsync(model.Code))
                {
                    ModelState.AddModelError("Code", "Mã giảm giá đã tồn tại");
                    return View("~/Views/Admin/Vouchers/Create.cshtml", model);
                }

                var result = await _voucherService.CreateVoucherAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Tạo mã giảm giá thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo mã giảm giá";
                }
            }

            return View("~/Views/Admin/Vouchers/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã giảm giá";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditVoucherViewModel
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Description = voucher.Description,
                DiscountPercent = voucher.DiscountPercent,
                MaxDiscount = voucher.MaxDiscount,
                MinOrderAmount = voucher.MinOrderAmount,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                UsageLimit = voucher.UsageLimit
            };

            return View("~/Views/Admin/Vouchers/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditVoucherViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu");
                    return View("~/Views/Admin/Vouchers/Edit.cshtml", model);
                }

                if (await _voucherService.IsCodeExistsAsync(model.Code, model.Id))
                {
                    ModelState.AddModelError("Code", "Mã giảm giá đã tồn tại");
                    return View("~/Views/Admin/Vouchers/Edit.cshtml", model);
                }

                var result = await _voucherService.UpdateVoucherAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật mã giảm giá thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật mã giảm giá";
                }
            }

            return View("~/Views/Admin/Vouchers/Edit.cshtml", model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã giảm giá";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/Vouchers/Details.cshtml", voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _voucherService.DeleteVoucherAsync(id);
            if (result)
            {
                return Json(new { success = true, message = "Xóa mã giảm giá thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa mã giảm giá" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckCodeExists(string code, int id = 0)
        {
            var exists = await _voucherService.IsCodeExistsAsync(code, id);
            return Json(!exists);
        }
    }
}
