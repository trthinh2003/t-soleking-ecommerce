using SoleKingECommerce.Data;
using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Product;
using SoleKingECommerce.ViewModels.ProductImage;

namespace SoleKingECommerce.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryService _categoryService;
        private readonly ILocalStorageService _localStorageService;
        private readonly ILogger<ProductService> _logger;
        private readonly SoleKingDbContext _context;

        public ProductService(
            IProductRepository productRepository,
            ICategoryService categoryService,
            ILocalStorageService localStorageService,
            ILogger<ProductService> logger, 
            SoleKingDbContext context)
        {
            _productRepository = productRepository;
            _categoryService = categoryService;
            _localStorageService = localStorageService;
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedList<ProductListViewModel>> GetPaginatedProductsAsync(
            string? searchString,
            int? categoryId,
            string? priceRange,
            int pageNumber,
            int pageSize)
        {
            return await _productRepository.GetPaginatedProductsAsync(
                searchString, categoryId, priceRange, pageNumber, pageSize);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetProductByIdWithDetailsAsync(id);
        }

        public async Task<ProductCreateViewModel> GetProductForCreateAsync()
        {
            var categories = await _categoryService.GetLeafCategoriesWithParentNamesAsync();

            return new ProductCreateViewModel
            {
                Categories = categories
            };
        }

        public async Task<ProductEditViewModel?> GetProductForEditAsync(int id)
        {
            var model = await _productRepository.GetProductForEditAsync(id);
            if (model != null)
            {
                model.Categories = await _categoryService.GetLeafCategoriesWithParentNamesAsync();
            }
            return model;
        }

        public async Task CreateProductAsync(ProductCreateViewModel model)
        {
            try
            {
                var product = new Product
                {
                    Name = model.Name,
                    Brand = model.Brand,
                    Description = model.Description,
                    BasePrice = model.BasePrice,
                    Tags = model.Tags,
                    CategoryId = model.CategoryId,
                    Slug = SlugHelper.GenerateSlug(model.Name ?? ""),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Handle main image upload
                if (model.MainImage != null)
                {
                    var imageUrl = await _localStorageService.UploadFileAsync(model.MainImage, "products");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        product.ImageUrl = imageUrl;
                    }
                }

                // Create product first
                var createdProduct = await _productRepository.CreateAsync(product);

                // Handle additional images
                if (model.AdditionalImages != null && model.AdditionalImages.Any())
                {
                    var productImages = new List<ProductImage>();

                    foreach (var additionalImage in model.AdditionalImages)
                    {
                        if (additionalImage != null)
                        {
                            var imageUrl = await _localStorageService.UploadFileAsync(additionalImage, "products");
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                productImages.Add(new ProductImage
                                {
                                    Name = additionalImage.FileName,
                                    ImageUrl = imageUrl,
                                    ProductId = createdProduct.Id,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                });
                            }
                        }
                    }

                    if (productImages.Any())
                    {
                        // You'll need to add a method to add product images
                        // or handle this through the context directly
                    }
                }

                _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ProductName}", model.Name);
                throw;
            }
        }

        public async Task UpdateProductAsync(ProductEditViewModel model)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(model.Id);
                if (product == null)
                {
                    throw new ArgumentException("Product not found");
                }
                product.Name = model.Name;
                product.Brand = model.Brand;
                product.Description = model.Description;
                product.BasePrice = model.BasePrice;
                product.Tags = model.Tags;
                product.CategoryId = model.CategoryId;
                product.Slug = SlugHelper.GenerateSlug(model.Name ?? "");
                product.UpdatedAt = DateTime.Now;

                // Xử lý thêm ảnh chính
                if (model.MainImage != null)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        _localStorageService.DeleteFile(product.ImageUrl);
                    }
                    var imageUrl = await _localStorageService.UploadFileAsync(model.MainImage, "products");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        product.ImageUrl = imageUrl;
                    }
                }

                // Xử lý thêm ảnh (ảnh mặt trước)
                if (model.FrontImage != null)
                {
                    var frontImageUrl = await _localStorageService.UploadFileAsync(model.FrontImage, "products");
                    if (!string.IsNullOrEmpty(frontImageUrl))
                    {
                        model.Images ??= new List<ProductImageEditViewModel>();
                        model.Images.Add(new ProductImageEditViewModel
                        {
                            Name = "Ảnh mặt trước",
                            ImageUrl = frontImageUrl,
                            ImageType = "Front"
                        });
                    }
                }

                // Xử lý thêm ảnh (ảnh mặt sau)
                if (model.BackImage != null)
                {
                    var backImageUrl = await _localStorageService.UploadFileAsync(model.BackImage, "products");
                    if (!string.IsNullOrEmpty(backImageUrl))
                    {
                        model.Images ??= new List<ProductImageEditViewModel>();
                        model.Images.Add(new ProductImageEditViewModel
                        {
                            Name = "Ảnh mặt sau",
                            ImageUrl = backImageUrl,
                            ImageType = "Back"
                        });
                    }
                }

                // Xử lý thêm ảnh (ảnh phụ)
                if (model.AdditionalImages != null && model.AdditionalImages.Any())
                {
                    model.Images ??= new List<ProductImageEditViewModel>();
                    foreach (var additionalImage in model.AdditionalImages)
                    {
                        if (additionalImage != null)
                        {
                            var imageUrl = await _localStorageService.UploadFileAsync(additionalImage, "products");
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                model.Images.Add(new ProductImageEditViewModel
                                {
                                    Name = additionalImage.FileName,
                                    ImageUrl = imageUrl,
                                    ImageType = "Additional"
                                });
                            }
                        }
                    }
                }

                await _productRepository.UpdateAsync(product);

                // Update variants
                if (model.Variants != null)
                {
                    await _productRepository.UpdateProductVariantsAsync(model.Id, model.Variants);
                }

                // Update images
                if (model.Images != null)
                {
                    await _productRepository.UpdateProductImagesAsync(model.Id, model.Images);
                }

                if (model.DeletedVariantIds?.Any() == true)
                {
                    await _productRepository.DeleteProductVariantsAsync(model.DeletedVariantIds);
                }

                if (model.DeletedImageIds?.Any() == true)
                {
                    await _productRepository.DeleteProductImagesAsync(model.DeletedImageIds);
                }

                _logger.LogInformation("Product updated successfully with ID: {ProductId}", product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", model.Id);
                throw;
            }
        }

        public async Task UpdateVariantStockAsync(int variantId, int quantity)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant != null)
            {
                variant.Stock += quantity; // quantity sẽ âm khi trừ stock
                variant.UpdatedAt = DateTime.Now;

                if (variant.Stock < 0)
                {
                    variant.Stock = 0;
                }

                _context.ProductVariants.Update(variant);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdWithDetailsAsync(id);
                if (product == null)
                {
                    throw new ArgumentException("Không tìm thấy sản phẩm");
                }

                // Xóa anh chinh
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    _localStorageService.DeleteFile(product.ImageUrl);
                }

                // Xóa ảnh phu
                if (product.Images != null && product.Images.Any())
                {
                    foreach (var image in product.Images)
                    {
                        if (!string.IsNullOrEmpty(image.ImageUrl))
                        {
                            _localStorageService.DeleteFile(image.ImageUrl);
                        }
                    }
                }

                await _productRepository.DeleteAsync(id);

                _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _productRepository.ProductExistsAsync(id);
        }
    }
}
