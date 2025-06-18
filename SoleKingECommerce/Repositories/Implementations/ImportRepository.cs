using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;

namespace SoleKingECommerce.Repositories.Implementations
{
    public class ImportRepository : IImportRepository
    {
        private readonly SoleKingDbContext _context;

        public ImportRepository(SoleKingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Import>> GetAllAsync()
        {
            return await _context.Imports
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Include(i => i.Items)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public IQueryable<Import> GetQueryable()
        {
            return _context.Imports
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Include(i => i.Items)
                .OrderByDescending(i => i.CreatedAt)
                .AsQueryable();
        }

        public async Task<Import?> GetByIdAsync(int id)
        {
            return await _context.Imports
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.ProductVariant)
                        .ThenInclude(pv => pv!.Color)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Import> CreateAsync(Import import)
        {
            _context.Imports.Add(import);
            await _context.SaveChangesAsync();
            return import;
        }

        public async Task<Import> UpdateAsync(Import import)
        {
            _context.Imports.Update(import);
            await _context.SaveChangesAsync();
            return import;
        }

        public async Task DeleteAsync(int id)
        {
            var import = await _context.Imports.FindAsync(id);
            if (import != null)
            {
                _context.Imports.Remove(import);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Import>> GetBySupplierAsync(int supplierId)
        {
            return await _context.Imports
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Where(i => i.SupplierId == supplierId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Import>> SearchAsync(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Imports
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Include(i => i.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i => i.Supplier!.Name!.Contains(searchString) ||
                                        i.Note!.Contains(searchString));
            }

            if (supplierId.HasValue)
            {
                query = query.Where(i => i.SupplierId == supplierId);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.CreatedAt >= fromDate);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.CreatedAt <= toDate);
            }

            return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }
    }
}
