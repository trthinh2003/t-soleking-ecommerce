using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;

namespace SoleKingECommerce.Repositories.Implementations
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly SoleKingDbContext _context;

        public ProductVariantRepository(SoleKingDbContext context)
        {
            _context = context;
        }

        public async Task<ProductVariant?> GetByProductColorSizeAsync(int productId, int colorId, string size)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product)
                .Include(pv => pv.Color)
                .FirstOrDefaultAsync(pv => pv.ProductId == productId &&
                                         pv.ColorId == colorId &&
                                         pv.Size == size);
        }

        public async Task<ProductVariant> CreateAsync(ProductVariant variant)
        {
            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task<ProductVariant> UpdateAsync(ProductVariant variant)
        {
            variant.UpdatedAt = DateTime.Now;
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task<IEnumerable<ProductVariant>> GetByProductAsync(int productId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Color)
                .Where(pv => pv.ProductId == productId)
                .ToListAsync();
        }
    }
}
