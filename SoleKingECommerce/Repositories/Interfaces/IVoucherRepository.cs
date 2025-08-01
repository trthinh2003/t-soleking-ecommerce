using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;

namespace SoleKingECommerce.Repositories.Interfaces
{
    public interface IVoucherRepository : IRepository<Voucher>
    {
        Task<PaginatedList<Voucher>> GetPaginatedAsync(int pageIndex, int pageSize, string searchString = null, string status = null);
        Task<Voucher> GetByCodeAsync(string code);
        Task<bool> IsCodeExistsAsync(string code, int excludeId = 0);
        Task<IEnumerable<Voucher>> GetActiveVouchersAsync();
        Task<IEnumerable<Voucher>> GetExpiredVouchersAsync();
        Task<int> GetUsageCountAsync(int voucherId);
    }
}
