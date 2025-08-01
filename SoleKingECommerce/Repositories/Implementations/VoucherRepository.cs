using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;

namespace SoleKingECommerce.Repositories.Implementations
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly SoleKingDbContext _context;

        public VoucherRepository(SoleKingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Voucher>> GetAllAsync()
        {
            return await _context.Vouchers
                .Include(v => v.VoucherUsages)
                .OrderByDescending(v => v.Id)
                .ToListAsync();
        }

        public async Task<Voucher> GetByIdAsync(int id)
        {
            return await _context.Vouchers
                .Include(v => v.VoucherUsages)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddAsync(Voucher entity)
        {
            await _context.Vouchers.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Voucher entity)
        {
            _context.Vouchers.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Voucher entity)
        {
            _context.Vouchers.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Vouchers.AnyAsync(v => v.Id == id);
        }

        public async Task<PaginatedList<Voucher>> GetPaginatedAsync(int pageIndex, int pageSize, string searchString = null, string status = null)
        {
            var query = _context.Vouchers
                .Include(v => v.VoucherUsages)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v => v.Code.Contains(searchString) ||
                                       v.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(status))
            {
                var now = DateTime.Now;
                switch (status.ToLower())
                {
                    case "active":
                        query = query.Where(v => v.StartDate <= now && v.EndDate >= now);
                        break;
                    case "expired":
                        query = query.Where(v => v.EndDate < now);
                        break;
                    case "upcoming":
                        query = query.Where(v => v.StartDate > now);
                        break;
                }
            }

            query = query.OrderByDescending(v => v.Id);

            return await PaginatedList<Voucher>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<Voucher> GetByCodeAsync(string code)
        {
            return await _context.Vouchers
                .Include(v => v.VoucherUsages)
                .FirstOrDefaultAsync(v => v.Code == code);
        }

        public async Task<bool> IsCodeExistsAsync(string code, int excludeId = 0)
        {
            return await _context.Vouchers
                .AnyAsync(v => v.Code == code && v.Id != excludeId);
        }

        public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync()
        {
            var now = DateTime.Now;
            return await _context.Vouchers
                .Where(v => v.StartDate <= now && v.EndDate >= now)
                .ToListAsync();
        }

        public async Task<IEnumerable<Voucher>> GetExpiredVouchersAsync()
        {
            var now = DateTime.Now;
            return await _context.Vouchers
                .Where(v => v.EndDate < now)
                .ToListAsync();
        }

        public async Task<int> GetUsageCountAsync(int voucherId)
        {
            return await _context.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucherId);
        }
    }
}
