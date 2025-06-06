using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Import;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;

namespace SoleKingECommerce.Services.Implementations
{
    public class ImportService : IImportService
    {
        private readonly IImportRepository _importRepository;
        private readonly IProductRepository _productRepository;
        private readonly SoleKingDbContext _context;

        public ImportService(IImportRepository importRepository, IProductRepository productRepository, SoleKingDbContext context)
        {
            _importRepository = importRepository;
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<IEnumerable<ImportListViewModel>> GetAllImportsAsync()
        {
            var imports = await _importRepository.GetAllAsync();
            return imports.Select(i => new ImportListViewModel
            {
                Id = i.Id,
                SupplierName = i.Supplier?.Name ?? "N/A",
                TotalPrice = i.TotalPrice,
                TotalItems = i.Items?.Count ?? 0,
                Note = i.Note,
                UserName = i.User?.Name ?? "N/A",
                CreatedAt = i.CreatedAt ?? DateTime.Now
            });
        }

        public async Task<ImportDetailsViewModel?> GetImportDetailsAsync(int id)
        {
            var import = await _importRepository.GetByIdAsync(id);
            if (import == null) return null;

            return new ImportDetailsViewModel
            {
                Id = import.Id,
                SupplierName = import.Supplier?.Name ?? "N/A",
                SupplierPhone = import.Supplier?.Phone ?? "N/A",
                SupplierEmail = import.Supplier?.Email ?? "N/A",
                TotalPrice = import.TotalPrice,
                Note = import.Note,
                UserName = import.User?.Name ?? "N/A",
                CreatedAt = import.CreatedAt ?? DateTime.Now,
                Items = import.Items?.Select(item => new ImportItemDetailsViewModel
                {
                    ProductName = item.ProductVariant?.Product?.Name ?? "N/A",
                    Brand = item.ProductVariant?.Product?.Brand ?? "N/A",
                    CategoryName = item.ProductVariant?.Product?.Category?.Name ?? "N/A",
                    ColorName = item.ProductVariant?.Color?.Name ?? "N/A",
                    Size = item.ProductVariant?.Size ?? "N/A",
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList() ?? new List<ImportItemDetailsViewModel>()
            };
        }

        public async Task<Import> CreateImportAsync(ImportCreateViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var import = new Import
                {
                    SupplierId = model.SupplierId,
                    Note = model.Note,
                    UserId = userId,
                    Items = new List<ImportItem>(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                decimal totalPrice = 0;

                foreach (var itemModel in model.Items)
                {
                    // Tìm hoặc tạo sản phẩm
                    var product = await _productRepository.GetByNameAndBrandAsync(itemModel.ProductName, itemModel.Brand);
                    if (product == null)
                    {
                        product = new Product
                        {
                            Name = itemModel.ProductName,
                            Brand = itemModel.Brand,
                            CategoryId = itemModel.CategoryId,
                            Description = itemModel.Description,
                            BasePrice = itemModel.SellPrice,
                            Slug = GenerateSlug(itemModel.ProductName),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        product = await _productRepository.CreateAsync(product);
                    }

                    // Tìm hoặc tạo variant
                    var variant = await _productRepository.GetVariantAsync(product.Id, itemModel.ColorId, itemModel.Size);
                    if (variant == null)
                    {
                        variant = new ProductVariant
                        {
                            ProductId = product.Id,
                            ColorId = itemModel.ColorId,
                            Size = itemModel.Size,
                            Price = itemModel.SellPrice,
                            Stock = itemModel.Quantity,
                            SKU = GenerateSKU(product.Name, itemModel.Size, itemModel.ColorId),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        variant = await _productRepository.CreateVariantAsync(variant);
                    }
                    else
                    {
                        // Cập nhật stock và giá
                        variant.Stock += itemModel.Quantity;
                        variant.Price = itemModel.SellPrice;
                        variant.UpdatedAt = DateTime.Now;
                        variant = await _productRepository.UpdateVariantAsync(variant);
                    }

                    // Tạo import item
                    var importItem = new ImportItem
                    {
                        ProductVariantId = variant.Id,
                        Quantity = itemModel.Quantity,
                        Price = itemModel.ImportPrice
                    };

                    import.Items.Add(importItem);
                    totalPrice += itemModel.ImportPrice * itemModel.Quantity;
                }
                import.TotalPrice = totalPrice;
                var result = await _importRepository.CreateAsync(import);

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteImportAsync(int id)
        {
            await _importRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ImportListViewModel>> SearchImportsAsync(string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate)
        {
            var imports = await _importRepository.SearchAsync(searchString, supplierId, fromDate, toDate);
            return imports.Select(i => new ImportListViewModel
            {
                Id = i.Id,
                SupplierName = i.Supplier?.Name ?? "N/A",
                TotalPrice = i.TotalPrice,
                TotalItems = i.Items?.Count ?? 0,
                Note = i.Note,
                UserName = i.User?.Name ?? "N/A",
                CreatedAt = i.CreatedAt ?? DateTime.Now
            });
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _productRepository.GetCategoriesAsync();
        }

        public async Task<IEnumerable<Color>> GetColorsAsync()
        {
            return await _productRepository.GetColorsAsync();
        }

        public async Task<IEnumerable<Supplier>> GetSuppliersAsync()
        {
            return await _productRepository.GetSuppliersAsync();
        }

        private string GenerateSlug(string name)
        {
            var vietnameseMap = new Dictionary<char, char>
            {
                ['á'] = 'a', ['à'] = 'a', ['ả'] = 'a', ['ã'] = 'a',
                ['ạ'] = 'a', ['â'] = 'a', ['ấ'] = 'a', ['ầ'] = 'a',
                ['ẩ'] = 'a', ['ẫ'] = 'a', ['ậ'] = 'a', ['ă'] = 'a',
                ['ắ'] = 'a', ['ằ'] = 'a', ['ẳ'] = 'a', ['ẵ'] = 'a', ['ặ'] = 'a', ['é'] = 'e',
                ['è'] = 'e', ['ẻ'] = 'e', ['ẽ'] = 'e', ['ẹ'] = 'e', ['ê'] = 'e', ['ế'] = 'e',
                ['ề'] = 'e', ['ể'] = 'e', ['ễ'] = 'e', ['ệ'] = 'e', ['í'] = 'i', ['ì'] = 'i',
                ['ỉ'] = 'i', ['ĩ'] = 'i', ['ị'] = 'i', ['ó'] = 'o', ['ò'] = 'o', ['ỏ'] = 'o',
                ['õ'] = 'o', ['ọ'] = 'o', ['ô'] = 'o', ['ố'] = 'o', ['ồ'] = 'o', ['ổ'] = 'o',
                ['ỗ'] = 'o', ['ộ'] = 'o', ['ơ'] = 'o', ['ớ'] = 'o', ['ờ'] = 'o', ['ở'] = 'o', 
                ['ỡ'] = 'o', ['ợ'] = 'o', ['ú'] = 'u', ['ù'] = 'u', ['ủ'] = 'u', ['ũ'] = 'u',
                ['ụ'] = 'u', ['ư'] = 'u', ['ứ'] = 'u', ['ừ'] = 'u',
                ['ử'] = 'u', ['ữ'] = 'u', ['ự'] = 'u', ['ý'] = 'y',
                ['ỳ'] = 'y', ['ỷ'] = 'y', ['ỹ'] = 'y', ['ỵ'] = 'y',
                ['đ'] = 'd'
            };

            var sb = new StringBuilder();
            foreach (var ch in name.ToLower())
            {
                if (vietnameseMap.ContainsKey(ch))
                    sb.Append(vietnameseMap[ch]);
                else if (char.IsLetterOrDigit(ch))
                    sb.Append(ch);
                else if (char.IsWhiteSpace(ch))
                    sb.Append('-');
                // bỏ qua ký tự đặc biệt khác
            }

            var result = sb.ToString();

            // loại bỏ dấu - đầu/cuối và dấu - trùng lặp
            result = Regex.Replace(result, "-{2,}", "-").Trim('-');

            return result;
        }


        private string GenerateSKU(string productName, string size, int colorId)
        {
            var shortName = string.Join("", productName.Split(' ').Select(w => w.FirstOrDefault()).Take(3));
            return $"{shortName.ToUpper()}-{size}-{colorId}-{DateTime.Now.Ticks.ToString().Substring(10)}";
        }
    }
}
