using SoleKingECommerce.Data;
using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Voucher;

namespace SoleKingECommerce.Services.Implementations
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly SoleKingDbContext _context;

        public VoucherService(IVoucherRepository voucherRepository, SoleKingDbContext context)
        {
            _voucherRepository = voucherRepository;
            _context = context;
        }

        public async Task<PaginatedList<VoucherViewModel>> GetPaginatedVouchersAsync(int pageIndex, int pageSize, string searchString = null, string status = null)
        {
            var paginatedVouchers = await _voucherRepository.GetPaginatedAsync(pageIndex, pageSize, searchString, status);

            var viewModels = paginatedVouchers.Select(v => new VoucherViewModel
            {
                Id = v.Id,
                Code = v.Code,
                Description = v.Description,
                DiscountPercent = v.DiscountPercent,
                MaxDiscount = v.MaxDiscount,
                MinOrderAmount = v.MinOrderAmount,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                UsageLimit = v.UsageLimit,
                UsageCount = v.VoucherUsages?.Count ?? 0,
                Status = GetVoucherStatus(v),
                IsActive = IsVoucherActive(v)
            }).ToList();

            return new PaginatedList<VoucherViewModel>(viewModels, paginatedVouchers.TotalCount, paginatedVouchers.PageIndex, paginatedVouchers.PageSize);
        }

        public async Task<VoucherViewModel> GetVoucherByIdAsync(int id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null) return null;

            return new VoucherViewModel
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Description = voucher.Description,
                DiscountPercent = voucher.DiscountPercent,
                MaxDiscount = voucher.MaxDiscount,
                MinOrderAmount = voucher.MinOrderAmount,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                UsageLimit = voucher.UsageLimit,
                UsageCount = voucher.VoucherUsages?.Count ?? 0,
                Status = GetVoucherStatus(voucher),
                IsActive = IsVoucherActive(voucher)
            };
        }

        public async Task<Voucher> GetByCodeAsync(string code)
        {
            return await _voucherRepository.GetByCodeAsync(code);
        }

        public async Task<bool> CreateVoucherAsync(CreateVoucherViewModel model)
        {
            try
            {
                if (await _voucherRepository.IsCodeExistsAsync(model.Code))
                    return false;

                var voucher = new Voucher
                {
                    Code = model.Code,
                    Description = model.Description,
                    DiscountPercent = model.DiscountPercent,
                    MaxDiscount = model.MaxDiscount,
                    MinOrderAmount = model.MinOrderAmount,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    UsageLimit = model.UsageLimit
                };

                await _voucherRepository.AddAsync(voucher);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateVoucherAsync(EditVoucherViewModel model)
        {
            try
            {
                var voucher = await _voucherRepository.GetByIdAsync(model.Id);
                if (voucher == null) return false;

                if (await _voucherRepository.IsCodeExistsAsync(model.Code, model.Id))
                    return false;

                voucher.Code = model.Code;
                voucher.Description = model.Description;
                voucher.DiscountPercent = model.DiscountPercent;
                voucher.MaxDiscount = model.MaxDiscount;
                voucher.MinOrderAmount = model.MinOrderAmount;
                voucher.StartDate = model.StartDate;
                voucher.EndDate = model.EndDate;
                voucher.UsageLimit = model.UsageLimit;

                await _voucherRepository.UpdateAsync(voucher);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> MarkAsUsedAsync(int voucherId, int orderId)
        {
            try
            {
                var voucherUsage = new VoucherUsage
                {
                    VoucherId = voucherId,
                    OrderId = orderId,
                    UsedAt = DateTime.Now
                };

                _context.VoucherUsages.Add(voucherUsage);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteVoucherAsync(int id)
        {
            try
            {
                var voucher = await _voucherRepository.GetByIdAsync(id);
                if (voucher == null) return false;

                await _voucherRepository.DeleteAsync(voucher);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsCodeExistsAsync(string code, int excludeId = 0)
        {
            return await _voucherRepository.IsCodeExistsAsync(code, excludeId);
        }

        public async Task<VoucherStatsViewModel> GetVoucherStatsAsync()
        {
            var allVouchers = await _voucherRepository.GetAllAsync();
            var activeVouchers = await _voucherRepository.GetActiveVouchersAsync();
            var expiredVouchers = await _voucherRepository.GetExpiredVouchersAsync();

            return new VoucherStatsViewModel
            {
                TotalVouchers = allVouchers.Count(),
                ActiveVouchers = activeVouchers.Count(),
                ExpiredVouchers = expiredVouchers.Count(),
                UpcomingVouchers = allVouchers.Count(v => v.StartDate > DateTime.Now)
            };
        }

        private string GetVoucherStatus(Voucher voucher)
        {
            var now = DateTime.Now;
            if (voucher.StartDate > now) return "Sắp diễn ra";
            if (voucher.EndDate < now) return "Hết hạn";
            return "Đang hoạt động";
        }

        private bool IsVoucherActive(Voucher voucher)
        {
            var now = DateTime.Now;
            return voucher.StartDate <= now && voucher.EndDate >= now;
        }
    }
}
