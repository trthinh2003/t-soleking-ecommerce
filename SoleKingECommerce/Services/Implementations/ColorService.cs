using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Services.Interfaces;

namespace SoleKingECommerce.Services.Implementations
{
    public class ColorService : IColorService
    {
        private readonly SoleKingDbContext _context;
        
        public ColorService(SoleKingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Color>> GetAllColorsAsync()
        {
            return await _context.Colors.ToListAsync();
        }
    }
}
