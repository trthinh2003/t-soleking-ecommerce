using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;

namespace SoleKingECommerce.Repositories.Implementations
{
    public class ColorRepository : IColorRepository
    {
        private readonly SoleKingDbContext _context;
        public ColorRepository(SoleKingDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Color>> GetAllAsync()
        {
            return await _context.Colors.ToListAsync();
        }

        public async Task AddAsync(Color entity)
        {
            await _context.Colors.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Color entity)
        {
            _context.Colors.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Color entity)
        {
            _context.Colors.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Color> GetByIdAsync(int id)
        {
            return await _context.Colors.FindAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Colors.AnyAsync(e => e.Id == id);
        }
    }
}
