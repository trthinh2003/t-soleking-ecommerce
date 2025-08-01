using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.ViewModels.Product;
using SoleKingECommerce.ViewModels.ProductImage;
using SoleKingECommerce.ViewModels.ProductVariant;

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

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<PaginatedList<ProductListViewModel>> GetPaginatedProductsAsync(string? searchString, int? categoryId, string? priceRange, int pageNumber, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) ||
                                       (p.Brand != null && p.Brand.Contains(searchString)));
            }
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            if (!string.IsNullOrEmpty(priceRange))
            {
                var parts = priceRange.Split('-');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out decimal minPrice) &&
                    decimal.TryParse(parts[1], out decimal maxPrice))
                {
                    query = query.Where(p => p.BasePrice >= minPrice && p.BasePrice <= maxPrice);
                }
            }
            var productQuery = query.Select(p => new ProductListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.Images.FirstOrDefault() != null ? p.Images.FirstOrDefault()!.ImageUrl : p.ImageUrl,
                Brand = p.Brand,
                Description = p.Description,
                BasePrice = p.BasePrice,
                Tags = p.Tags,
                Slug = p.Slug,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "",
                VariantCount = p.Variants.Count(),
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = p.UpdatedAt ?? DateTime.MinValue
            });
            productQuery = productQuery.OrderByDescending(p => p.CreatedAt);

            return await PaginatedList<ProductListViewModel>.CreateAsync(productQuery, pageNumber, pageSize);
        }

        public async Task<Product?> GetProductByIdWithDetailsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Color)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProductEditViewModel?> GetProductForEditAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Color)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            return new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Description = product.Description,
                BasePrice = product.BasePrice,
                Tags = product.Tags,
                CategoryId = product.CategoryId,
                CurrentImageUrl = product.ImageUrl,
                ExistingImages = product.Images.Select(img => new ProductImageViewModel
                {
                    Id = img.Id,
                    Name = img.Name,
                    ImageUrl = img.ImageUrl,
                    ProductId = img.ProductId
                }).ToList(),
                Variants = product.Variants.Select(v => new ProductVariantEditViewModel
                {
                    Id = v.Id,
                    Size = v.Size,
                    Price = v.Price,
                    Stock = v.Stock,
                    SKU = v.SKU,
                    ColorId = v.ColorId,
                    ColorName = v.Color?.Name
                }).ToList(),
                Images = product.Images.Select(i => new ProductImageEditViewModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    ImageUrl = i.ImageUrl,
                    ImageType = DetermineImageType(i.Name)
                }).ToList()
            };
        }

        private string DetermineImageType(string? imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return "Additional";
            if (imageName.Contains("mặt trước", StringComparison.OrdinalIgnoreCase) ||
                imageName.Contains("front", StringComparison.OrdinalIgnoreCase))
                return "Front";
            if (imageName.Contains("mặt sau", StringComparison.OrdinalIgnoreCase) ||
                imageName.Contains("back", StringComparison.OrdinalIgnoreCase))
                return "Back";
            return "Additional";
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                _context.ProductImages.RemoveRange(product.Images);
                _context.ProductVariants.RemoveRange(product.Variants);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }


        #region IRepository<Product> implementation
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id) ?? new Product();
        }

        public async Task AddAsync(Product entity)
        {
            await _context.Products.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        Task IRepository<Product>.UpdateAsync(Product entity)
        {
            return UpdateAsync(entity);
        }

        public async Task DeleteAsync(Product entity)
        {
            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(e => e.Id == id);
        }
        #endregion

        public async Task<List<ProductVariant>> GetProductVariantsAsync(int productId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Color)
                .Where(pv => pv.ProductId == productId)
                .ToListAsync();
        }

        public async Task<List<ProductImage>> GetProductImagesAsync(int productId)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();
        }

        public async Task UpdateProductVariantsAsync(int productId, List<ProductVariantEditViewModel> variants)
        {
            var existingVariants = await _context.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .ToListAsync();

            foreach (var variant in variants.Where(v => !v.IsDeleted))
            {
                if (variant.Id == 0) // New variant
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = productId,
                        Size = variant.Size,
                        Price = variant.Price,
                        Stock = variant.Stock,
                        SKU = variant.SKU,
                        ColorId = variant.ColorId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                else // Sửa lại nếu đã tồn tại
                {
                    var existing = existingVariants.FirstOrDefault(ev => ev.Id == variant.Id);
                    if (existing != null)
                    {
                        existing.Size = variant.Size;
                        existing.Price = variant.Price;
                        existing.Stock = variant.Stock;
                        existing.SKU = variant.SKU;
                        existing.ColorId = variant.ColorId;
                        existing.UpdatedAt = DateTime.Now;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductImagesAsync(int productId, List<ProductImageEditViewModel> images)
        {
            var existingImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            foreach (var image in images.Where(i => !i.IsDeleted))
            {
                if (image.Id == 0) // New image
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = productId,
                        Name = image.Name,
                        ImageUrl = image.ImageUrl,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                else // Update existing
                {
                    var existing = existingImages.FirstOrDefault(ei => ei.Id == image.Id);
                    if (existing != null)
                    {
                        existing.Name = image.Name;
                        existing.UpdatedAt = DateTime.Now;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductVariantsAsync(List<int> variantIds)
        {
            if (variantIds?.Any() == true)
            {
                var variants = await _context.ProductVariants
                    .Where(pv => variantIds.Contains(pv.Id))
                    .ToListAsync();
                _context.ProductVariants.RemoveRange(variants);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductImagesAsync(List<int> imageIds)
        {
            if (imageIds?.Any() == true)
            {
                var images = await _context.ProductImages
                    .Where(pi => imageIds.Contains(pi.Id))
                    .ToListAsync();
                _context.ProductImages.RemoveRange(images);
                await _context.SaveChangesAsync();
            }
        }
    }
}
