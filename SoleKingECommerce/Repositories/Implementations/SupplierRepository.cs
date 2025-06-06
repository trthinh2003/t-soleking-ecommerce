using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;

namespace SoleKingECommerce.Repositories.Implementations
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly SoleKingDbContext _context;

        public SupplierRepository(SoleKingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            var suppliers = await _context.Suppliers.ToListAsync();
            return suppliers;
        }

        public async Task<Supplier> GetByIdAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            return supplier;
        }

        public IQueryable<Supplier> GetQueryable() => _context.Suppliers.AsQueryable();

        public async Task AddAsync(Supplier entity)
        {
            await _context.Suppliers.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Supplier entity)
        {
            _context.Suppliers.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Supplier entity)
        {
            _context.Suppliers.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Suppliers.AnyAsync(e => e.Id == id);
        }
    }
}
