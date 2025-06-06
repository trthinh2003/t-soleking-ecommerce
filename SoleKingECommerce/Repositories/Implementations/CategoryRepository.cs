using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;

namespace SoleKingECommerce.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly SoleKingDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;
        public CategoryRepository(SoleKingDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllWithProductsAsync()
        {
            return await _context.Categories.Include(c => c.Products).ToListAsync();
        }

        public IQueryable<Category> GetQueryableWithProducts()
        {
            return _context.Categories.Include(c => c.Products).AsQueryable();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> GetByIdWithProductsAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id) ?? new Category();
        }

        public async Task AddAsync(Category entity)
        {
            await _context.Categories.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category entity)
        {
            _context.Categories.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category entity)
        {
            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Category>> GetParentCategoriesAsync(int? excludeId = null)
        {
            var query = _context.Categories.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }
    }
}
