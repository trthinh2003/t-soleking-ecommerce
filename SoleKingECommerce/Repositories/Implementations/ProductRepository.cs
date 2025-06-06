using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;

namespace SoleKingECommerce.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly SoleKingDbContext _context;

        public ProductRepository(SoleKingDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByNameAndBrandAsync(string name, string brand)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Name == name && p.Brand == brand);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<ProductVariant?> GetVariantAsync(int productId, int colorId, string size)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(pv => pv.ProductId == productId &&
                                          pv.ColorId == colorId &&
                                          pv.Size == size);
        }

        public async Task<ProductVariant> CreateVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task<ProductVariant> UpdateVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<IEnumerable<Color>> GetColorsAsync()
        {
            return await _context.Colors.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> GetSuppliersAsync()
        {
            return await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
        }
    }
}
