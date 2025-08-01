using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.ViewModels.Voucher;

namespace SoleKingECommerce.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<PaginatedList<VoucherViewModel>> GetPaginatedVouchersAsync(int pageIndex, int pageSize, string searchString = null, string status = null);
        Task<VoucherViewModel> GetVoucherByIdAsync(int id);
        Task<Voucher> GetByCodeAsync(string code);
        Task<bool> CreateVoucherAsync(CreateVoucherViewModel model);
        Task<bool> UpdateVoucherAsync(EditVoucherViewModel model);
        Task<bool> MarkAsUsedAsync(int voucherId, int orderId);
        Task<bool> DeleteVoucherAsync(int id);
        Task<bool> IsCodeExistsAsync(string code, int excludeId = 0);
        Task<VoucherStatsViewModel> GetVoucherStatsAsync();
    }
}
