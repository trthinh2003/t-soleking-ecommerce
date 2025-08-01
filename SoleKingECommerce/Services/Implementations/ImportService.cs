using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Import;
using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Data;
using SoleKingECommerce.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using QuestPDF.Drawing;

namespace SoleKingECommerce.Services.Implementations
{
    public class ImportService : IImportService
    {
        private readonly IImportRepository _importRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILocalStorageService _localStorageService;
        private readonly SoleKingDbContext _context;
        private readonly ILogger<ImportService> _logger;

        public ImportService(
            IImportRepository importRepository,
            IProductRepository productRepository,
            ILocalStorageService localStorageService,
            SoleKingDbContext context,
            ILogger<ImportService> logger
        )
        {
            _importRepository = importRepository;
            _productRepository = productRepository;
            _localStorageService = localStorageService;
            _context = context;
            _logger = logger;
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

        public async Task<PaginatedList<ImportListViewModel>> GetPaginatedImportsAsync(string searchString, int? supplierId, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            var query = _importRepository.GetQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i => i.Supplier.Name.Contains(searchString));
            }

            if (supplierId.HasValue)
            {
                query = query.Where(i => i.SupplierId == supplierId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.CreatedAt <= toDate.Value);
            }

            return await PaginatedList<ImportListViewModel>.CreateAsync(
                query.Select(i => new ImportListViewModel
                {
                    Id = i.Id,
                    SupplierName = i.Supplier.Name ?? "N/A",
                    TotalPrice = i.TotalPrice,
                    TotalItems = i.Items.Count,
                    Note = i.Note,
                    UserName = i.User.Name ?? "N/A",
                    CreatedAt = i.CreatedAt ?? DateTime.Now
                }),
                page,
                pageSize
            );
        }


        public async Task<ImportDetailsViewModel?> GetImportDetailsAsync(int id)
        {
            var import = await _importRepository.GetByIdAsync(id);
            if (import == null) return null;

            return new ImportDetailsViewModel
            {
                Id = import.Id,
                SupplierId = import.SupplierId,
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
                    ImageUrl = item.ProductVariant?.Product?.ImageUrl ?? "N/A",
                    Brand = item.ProductVariant?.Product?.Brand ?? "N/A",
                    CategoryId = item.ProductVariant?.Product?.CategoryId ?? 0,
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
                            BasePrice = 0.0m,
                            Slug = SlugHelper.GenerateSlug(itemModel.ProductName),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        // LƯU SẢN PHẨM TRƯỚC KHI XỬ LÝ ẢNH
                        product = await _productRepository.CreateAsync(product);
                        await _context.SaveChangesAsync(); // Đảm bảo product.Id đã được tạo

                        // Xử lý upload ảnh - ĐÂY LÀ PHẦN QUAN TRỌNG NHẤT
                        if (itemModel.Images != null && itemModel.Images.Length > 0)
                        {
                            _logger.LogInformation($"Processing {itemModel.Images.Length} images for product {product.Id}");

                            var productImages = new List<ProductImage>();
                            string? mainImageUrl = null;

                            for (int i = 0; i < itemModel.Images.Length; i++)
                            {
                                var image = itemModel.Images[i];
                                if (image != null && image.Length > 0)
                                {
                                    _logger.LogInformation($"Uploading image {i + 1}: {image.FileName}, Size: {image.Length}");

                                    var imageUrl = await _localStorageService.UploadFileAsync(image, "products");
                                    if (!string.IsNullOrEmpty(imageUrl))
                                    {
                                        _logger.LogInformation($"Image uploaded successfully: {imageUrl}");

                                        var productImage = new ProductImage
                                        {
                                            ProductId = product.Id,
                                            ImageUrl = imageUrl,
                                            CreatedAt = DateTime.Now
                                        };

                                        productImages.Add(productImage);

                                        // Ảnh đầu tiên làm ảnh chính
                                        if (i == 0)
                                        {
                                            mainImageUrl = imageUrl;
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Failed to upload image {i + 1}");
                                    }
                                }
                            }

                            // Lưu tất cả ảnh vào database
                            if (productImages.Any())
                            {
                                await _context.ProductImages.AddRangeAsync(productImages);
                                await _context.SaveChangesAsync();

                                _logger.LogInformation($"Saved {productImages.Count} images to database for product {product.Id}");

                                // Cập nhật ảnh chính cho sản phẩm
                                if (!string.IsNullOrEmpty(mainImageUrl))
                                {
                                    product.ImageUrl = mainImageUrl;
                                    await _productRepository.UpdateAsync(product);
                                    await _context.SaveChangesAsync();

                                    _logger.LogInformation($"Updated main image for product {product.Id}: {mainImageUrl}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"No images were successfully processed for product {product.Id}");
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"No images provided for product {product.Id}");
                        }
                    }

                    // Xử lý các thông số kỹ thuật (specifications)
                    var parsedSpecs = itemModel.ParsedSpecifications;

                    foreach (var spec in parsedSpecs)
                    {
                        // Tìm hoặc tạo variant
                        var variant = await _productRepository.GetVariantAsync(product.Id, spec.ColorId, spec.Size);
                        if (variant == null)
                        {
                            variant = new ProductVariant
                            {
                                ProductId = product.Id,
                                ColorId = spec.ColorId,
                                Size = spec.Size,
                                Price = 0.0m,
                                Stock = spec.Quantity,
                                SKU = GenerateSKU(product.Name, spec.Size, spec.ColorId),
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            variant = await _productRepository.CreateVariantAsync(variant);
                        }
                        else
                        {
                            // Cập nhật stock và giá
                            variant.Stock += spec.Quantity;
                            variant.Price = 0.0m;
                            variant.UpdatedAt = DateTime.Now;
                            variant = await _productRepository.UpdateVariantAsync(variant);
                        }

                        // Tạo import item
                        var importItem = new ImportItem
                        {
                            ProductVariantId = variant.Id,
                            Quantity = spec.Quantity,
                            Price = itemModel.ImportPrice
                        };

                        import.Items.Add(importItem);
                        totalPrice += itemModel.ImportPrice * spec.Quantity;
                    }
                }

                import.TotalPrice = totalPrice;
                var result = await _importRepository.CreateAsync(import);

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateImportAsync");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreateImportFromExistingAsync(int originalImportId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldImport = await _importRepository.GetByIdAsync(originalImportId);
                if (oldImport == null) throw new Exception("Không tìm thấy phiếu nhập gốc");

                var newImport = new Import
                {
                    SupplierId = oldImport.SupplierId,
                    Note = $"Nhập bổ sung từ phiếu #{originalImportId}",
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Items = new List<ImportItem>()
                };

                decimal totalPrice = 0;
                var reference = new ImportReference
                {
                    FromImportId = originalImportId,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<ImportReferenceItem>()
                };

                foreach (var item in oldImport.Items!)
                {
                    var variant = item.ProductVariant!;
                    variant.Stock += item.Quantity;
                    variant.UpdatedAt = DateTime.Now;
                    await _productRepository.UpdateVariantAsync(variant);

                    var newItem = new ImportItem
                    {
                        ProductVariantId = variant.Id,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    newImport.Items.Add(newItem);
                    totalPrice += item.Quantity * item.Price;

                    reference.Items.Add(new ImportReferenceItem
                    {
                        ProductVariantId = variant.Id,
                        QuantityAdded = item.Quantity,
                        Note = $"Sao chép từ phiếu #{originalImportId}"
                    });
                }

                newImport.TotalPrice = totalPrice;
                var createdImport = await _importRepository.CreateAsync(newImport);
                reference.ToImportId = createdImport.Id;

                _context.ImportReferences.Add(reference);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
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

        public async Task<IEnumerable<object>> GetLeafCategoriesWithParentNamesAsync()
        {
            var allCategories = await GetCategoriesAsync();

            var categoryDict = allCategories.ToDictionary(c => c.Id);

            var result = new List<object>();

            foreach (var category in allCategories)
            {
                bool isLeaf = !allCategories.Any(c => c.ParentId == category.Id);

                if (isLeaf && category.ParentId.HasValue)
                {
                    if (categoryDict.TryGetValue(category.ParentId.Value, out var parentCategory))
                    {
                        result.Add(new
                        {
                            Id = category.Id,
                            Name = $"{parentCategory.Name} - {category.Name}"
                        });
                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<Models.Color>> GetColorsAsync()
        {
            return await _productRepository.GetColorsAsync();
        }

        public async Task<IEnumerable<Supplier>> GetSuppliersAsync()
        {
            return await _productRepository.GetSuppliersAsync();
        }
       
        private string GenerateSKU(string productName, string size, int colorId)
        {
            var shortName = string.Join("", productName.Split(' ').Select(w => w.FirstOrDefault()).Take(3));
            return $"{shortName.ToUpper()}-{size}-{colorId}-{DateTime.Now.Ticks.ToString().Substring(10)}";
        }

        private bool IsValidSpecification(string specification)
        {
            if (string.IsNullOrEmpty(specification)) return false;

            var parts = specification.Split('-');
            if (parts.Length < 3) return false;

            return int.TryParse(parts[0], out int colorId) &&
                   colorId > 0 &&
                   !string.IsNullOrEmpty(parts[1]) &&
                   int.TryParse(parts[2], out int quantity) &&
                   quantity > 0;
        }

        /*=================================PHẦN XỬ LÝ XEM LỊCH SỬ NHẬP HÀNG=================================*/
        public async Task<PaginatedList<ImportHistoryViewModel>> GetImportHistoryAsync(
        string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            var query = from import in _context.Imports
                        join reference in _context.ImportReferences on import.Id equals reference.ToImportId into refGroup
                        from reference in refGroup.DefaultIfEmpty()
                        select new { import, reference };

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.import.Supplier.Name.Contains(searchString) ||
                                       x.import.Note.Contains(searchString));
            }

            if (supplierId.HasValue)
            {
                query = query.Where(x => x.import.SupplierId == supplierId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.import.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.import.CreatedAt <= toDate.Value);
            }

            var historyQuery = query.Select(x => new ImportHistoryViewModel
            {
                Id = x.import.Id,
                SupplierName = x.import.Supplier.Name ?? "N/A",
                TotalPrice = x.import.TotalPrice,
                TotalItems = x.import.Items.Count,
                Note = x.import.Note,
                UserName = x.import.User.Name ?? "N/A",
                CreatedAt = x.import.CreatedAt ?? DateTime.Now,
                IsFromReference = x.reference != null,
                OriginalImportId = x.reference != null ? x.reference.FromImportId : (int?)null,
                ReferenceNote = x.reference != null ? $"Nhập bổ sung từ phiếu #{x.reference.FromImportId}" : null
            }).OrderByDescending(x => x.CreatedAt);

            return await PaginatedList<ImportHistoryViewModel>.CreateAsync(historyQuery, page, pageSize);
        }

        public async Task<ImportHistoryDetailViewModel?> GetImportHistoryDetailAsync(int importId)
        {
            var import = await _context.Imports
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.ProductVariant)
                        .ThenInclude(pv => pv.Color)
                .FirstOrDefaultAsync(i => i.Id == importId);

            if (import == null) return null;

            // Get reference info
            var reference = await _context.ImportReferences
                .Include(r => r.FromImport)
                    .ThenInclude(fi => fi.Supplier)
                .Include(r => r.Items)
                    .ThenInclude(ri => ri.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(r => r.Items)
                    .ThenInclude(ri => ri.ProductVariant)
                        .ThenInclude(pv => pv.Color)
                .FirstOrDefaultAsync(r => r.ToImportId == importId);

            // Get related imports (imports created from this one)
            var relatedImports = await _context.ImportReferences
                .Where(r => r.FromImportId == importId)
                .Include(r => r.ToImport)
                    .ThenInclude(ti => ti.Supplier)
                .Include(r => r.ToImport)
                    .ThenInclude(ti => ti.User)
                .Select(r => new RelatedImportViewModel
                {
                    Id = r.ToImport.Id,
                    SupplierName = r.ToImport.Supplier.Name ?? "N/A",
                    TotalPrice = r.ToImport.TotalPrice,
                    TotalItems = r.ToImport.Items.Count,
                    UserName = r.ToImport.User.Name ?? "N/A",
                    CreatedAt = r.ToImport.CreatedAt ?? DateTime.Now
                })
                .ToListAsync();

            return new ImportHistoryDetailViewModel
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
                    ImageUrl = item.ProductVariant?.Product?.ImageUrl ?? "N/A",
                    Brand = item.ProductVariant?.Product?.Brand ?? "N/A",
                    CategoryId = item.ProductVariant?.Product?.CategoryId ?? 0,
                    CategoryName = item.ProductVariant?.Product?.Category?.Name ?? "N/A",
                    ColorName = item.ProductVariant?.Color?.Name ?? "N/A",
                    Size = item.ProductVariant?.Size ?? "N/A",
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList() ?? new List<ImportItemDetailsViewModel>(),

                // Reference information
                IsFromReference = reference != null,
                OriginalImport = reference != null ? new OriginalImportViewModel
                {
                    Id = reference.FromImport.Id,
                    SupplierName = reference.FromImport.Supplier?.Name ?? "N/A",
                    TotalPrice = reference.FromImport.TotalPrice,
                    CreatedAt = reference.FromImport.CreatedAt ?? DateTime.Now,
                    AddedItems = reference.Items?.Select(ri => new AddedItemViewModel
                    {
                        ProductName = ri.ProductVariant?.Product?.Name ?? "N/A",
                        ColorName = ri.ProductVariant?.Color?.Name ?? "N/A",
                        Size = ri.ProductVariant?.Size ?? "N/A",
                        QuantityAdded = ri.QuantityAdded,
                        Note = ri.Note
                    }).ToList() ?? new List<AddedItemViewModel>()
                } : null,

                RelatedImports = relatedImports
            };
        }

        public async Task<IEnumerable<ImportHistoryExportViewModel>> GetImportHistoryForExportAsync(
            string? searchString, int? supplierId, DateTime? fromDate, DateTime? toDate)
        {
            var query = from import in _context.Imports
                        join reference in _context.ImportReferences on import.Id equals reference.ToImportId into refGroup
                        from reference in refGroup.DefaultIfEmpty()
                        select new { import, reference };

            // Apply same filters as History method
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.import.Supplier.Name.Contains(searchString) ||
                                       x.import.Note.Contains(searchString));
            }

            if (supplierId.HasValue)
            {
                query = query.Where(x => x.import.SupplierId == supplierId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.import.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.import.CreatedAt <= toDate.Value);
            }

            return await query.Select(x => new ImportHistoryExportViewModel
            {
                ImportId = x.import.Id,
                SupplierName = x.import.Supplier.Name ?? "N/A",
                TotalPrice = x.import.TotalPrice,
                TotalItems = x.import.Items.Count,
                Note = x.import.Note ?? "",
                UserName = x.import.User.Name ?? "N/A",
                CreatedAt = x.import.CreatedAt ?? DateTime.Now,
                IsFromReference = x.reference != null,
                OriginalImportId = x.reference != null ? x.reference.FromImportId.ToString() : "",
                ReferenceType = x.reference != null ? "Nhập bổ sung" : "Nhập mới"
            }).OrderByDescending(x => x.CreatedAt).ToListAsync();
        }


        public async Task<byte[]> ExportHistoryToExcelAsync(IEnumerable<ImportHistoryExportViewModel> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Lịch sử nhập hàng");

                // === Tạo chú thích về màu sắc ===
                var noteRow = worksheet.Row(1);
                noteRow.Cell(1).Value = "CHÚ THÍCH:";
                noteRow.Cell(2).Value = "Các dòng có ghi chú sẽ được đánh dấu màu cam";
                noteRow.Cell(2).Style.Fill.BackgroundColor = XLColor.LightGoldenrodYellow;
                noteRow.Style.Font.Bold = true;
                noteRow.Height = 20;

                int currentRow = 2; // Bắt đầu từ dòng 2 do dòng 1 dành cho chú thích

                // === Tiêu đề chính ===
                worksheet.Cell(currentRow, 1).Value = "LỊCH SỬ NHẬP HÀNG";
                worksheet.Range(currentRow, 1, currentRow, 9).Merge().Style
                    .Font.SetBold().Font.FontSize = 16;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                currentRow += 2;

                // === Header ===
                var headers = new[]
                {
                    "Mã phiếu", "Nhà cung cấp", "Tổng tiền", "Số lượng SP",
                    "Ghi chú", "Người tạo", "Ngày tạo", "Loại nhập", "Phiếu gốc"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = headers[i];
                }

                var headerRange = worksheet.Range(currentRow, 1, currentRow, headers.Length);
                headerRange.Style
                    .Font.SetBold()
                    .Font.SetFontName("Tahoma")
                    .Fill.SetBackgroundColor(XLColor.FromArgb(221, 235, 247))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.SheetView.FreezeRows(currentRow);
                currentRow++;

                // === Dữ liệu ===
                var groupedData = data.GroupBy(x => x.ImportId).ToList();
                bool isOddRow = true;

                foreach (var group in groupedData)
                {
                    int groupStartRow = currentRow;
                    var groupItems = group.ToList();

                    foreach (var item in groupItems)
                    {
                        // Xác định màu nền
                        var bgColor = isOddRow
                            ? XLColor.FromArgb(242, 242, 242) // Xám nhạt cho dòng lẻ
                            : XLColor.White; // Trắng cho dòng chẵn

                        // ĐÁNH DẤU MÀU CAM CHO DÒNG CÓ GHI CHÚ
                        // (Màu LightGoldenrodYellow là màu cam nhạt)
                        if (!string.IsNullOrWhiteSpace(item.Note))
                        {
                            bgColor = XLColor.LightGoldenrodYellow;
                        }

                        var rowRange = worksheet.Range(currentRow, 1, currentRow, 9);
                        rowRange.Style.Fill.BackgroundColor = bgColor;

                        // Ghi dữ liệu
                        worksheet.Cell(currentRow, 1).Value = $"#{item.ImportId}";
                        worksheet.Cell(currentRow, 2).Value = item.SupplierName ?? "";
                        worksheet.Cell(currentRow, 3).Value = item.TotalPrice;
                        worksheet.Cell(currentRow, 4).Value = item.TotalItems;
                        worksheet.Cell(currentRow, 5).Value = item.Note ?? "";
                        worksheet.Cell(currentRow, 6).Value = item.UserName ?? "";

                        if (item.CreatedAt != null)
                        {
                            worksheet.Cell(currentRow, 7).Value = item.CreatedAt;
                            worksheet.Cell(currentRow, 7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 7).Value = "";
                        }

                        worksheet.Cell(currentRow, 8).Value = item.ReferenceType ?? "";
                        worksheet.Cell(currentRow, 9).Value = item.OriginalImportId ?? "";

                        isOddRow = !isOddRow;
                        currentRow++;
                    }

                    // Gộp các cell theo ImportId
                    if (groupItems.Count > 1)
                    {
                        try
                        {
                            var columnsToMerge = new[] { 1, 2, 3, 4, 6, 8, 9 };

                            foreach (int col in columnsToMerge)
                            {
                                var firstCellValue = worksheet.Cell(groupStartRow, col).Value;
                                bool allSame = true;

                                for (int row = groupStartRow + 1; row < currentRow; row++)
                                {
                                    if (!worksheet.Cell(row, col).Value.Equals(firstCellValue))
                                    {
                                        allSame = false;
                                        break;
                                    }
                                }

                                if (allSame)
                                {
                                    worksheet.Range(groupStartRow, col, currentRow - 1, col).Merge();
                                    worksheet.Cell(groupStartRow, col).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi gộp cell: {ex.Message}");
                        }
                    }
                }

                // === Định dạng ===
                try
                {
                    worksheet.Column(3).Style.NumberFormat.Format = "#,##0 \"đ\"";
                    worksheet.Column(4).Style.NumberFormat.Format = "0";

                    // Áp dụng border cho toàn bộ dữ liệu
                    var dataRange = worksheet.Range(3, 1, currentRow - 1, 9);
                    dataRange.Style
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                        .Border.SetInsideBorder(XLBorderStyleValues.Thin);

                    // Điều chỉnh độ rộng cột
                    worksheet.Columns().AdjustToContents();
                    foreach (var col in worksheet.Columns())
                    {
                        if (col.Width > 50)
                            col.Width = 50;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi định dạng: {ex.Message}");
                }

                // === Ghi chú tổng số dòng ===
                worksheet.Cell(currentRow + 1, 1).Value = $"Tổng số dòng: {data.Count()}";
                worksheet.Cell(currentRow + 1, 1).Style.Font.Italic = true;

                // === Xuất file ===
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }


        public async Task<byte[]> ExportHistoryToPdfAsync(IEnumerable<ImportHistoryExportViewModel> data)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                // Load Vietnamese font
                var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "BeVietnamPro-Light.ttf");
                using var fontStream = File.OpenRead(fontPath);
                FontManager.RegisterFont(fontStream);

                // Vietnamese font styling
                var vietnameseFont = TextStyle.Default
                    .FontFamily("Arial")
                    .Fallback(TextStyle.Default.FontFamily("Arial"))
                    .Fallback(TextStyle.Default.FontFamily("Times New Roman"))
                    .Fallback(TextStyle.Default.FontFamily("Segoe UI"));

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Portrait());
                        page.Margin(20, Unit.Millimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(vietnameseFont.FontSize(10));

                        // HEADER - Simplified and elegant
                        page.Header()
                            .Height(70)
                            .Padding(15)
                            .Column(headerColumn =>
                            {
                                headerColumn.Item().AlignCenter().Text("BÁO CÁO LỊCH SỬ NHẬP HÀNG")
                                    .Style(vietnameseFont.SemiBold().FontSize(18).FontColor(Colors.Blue.Darken3));

                                headerColumn.Item().AlignCenter().Text("T-SOLEKING MANAGEMENT SYSTEM")
                                    .Style(vietnameseFont.FontSize(11).FontColor(Colors.Grey.Darken2));

                                //headerColumn.Item().AlignCenter().PaddingTop(5).Text($"Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                //    .Style(vietnameseFont.FontSize(9).FontColor(Colors.Grey.Darken1));
                            });

                        // STATS SECTION - Clean and separated
                        page.Content()
                            .PaddingTop(10)
                            .Row(statsRow =>
                            {
                                statsRow.RelativeItem().Column(statsColumn =>
                                {
                                    statsColumn.Item().PaddingBottom(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Tổng bản ghi: {data.Count()}")
                                            .Style(vietnameseFont.SemiBold().FontSize(10).FontColor(Colors.Grey.Darken2));

                                        var totalAmount = data.Sum(x => x.TotalPrice);
                                        row.RelativeItem().AlignRight().Text($"Tổng giá trị: {totalAmount:N0} đ")
                                            .Style(vietnameseFont.SemiBold().FontSize(10).FontColor(Colors.Green.Darken2));
                                    });

                                    statsColumn.Item().LineHorizontal(1).LineColor(Colors.Blue.Darken3);
                                });
                            });

                        // DATA TABLE - Professional and clean
                        page.Content()
                            .PaddingTop(5)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(0.8f);  // Mã phiếu
                                    columns.RelativeColumn(2.2f);  // Nhà cung cấp
                                    columns.RelativeColumn(1.3f);  // Tổng tiền
                                    columns.RelativeColumn(0.8f);  // SL SP
                                    columns.RelativeColumn(2.0f);  // Ghi chú
                                    columns.RelativeColumn(1.5f);  // Người tạo
                                    columns.RelativeColumn(1.8f);  // Ngày tạo
                                    columns.RelativeColumn(1.2f);  // Loại
                                });

                                // Table header
                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Mã phiếu");
                                    header.Cell().Element(HeaderCellStyle).Text("Nhà cung cấp");
                                    header.Cell().Element(HeaderCellStyle).Text("Tổng tiền");
                                    header.Cell().Element(HeaderCellStyle).Text("SL SP");
                                    header.Cell().Element(HeaderCellStyle).Text("Ghi chú");
                                    header.Cell().Element(HeaderCellStyle).Text("Người tạo");
                                    header.Cell().Element(HeaderCellStyle).Text("Ngày tạo");
                                    header.Cell().Element(HeaderCellStyle).Text("Loại");

                                    IContainer HeaderCellStyle(IContainer container)
                                    {
                                        return container
                                            .Background(Colors.Blue.Darken3)
                                            .Padding(8)
                                            .DefaultTextStyle(vietnameseFont.SemiBold().FontSize(10).FontColor(Colors.White))
                                            .AlignCenter();
                                    }
                                });

                                // Table rows
                                int rowIndex = 0;
                                foreach (var item in data)
                                {
                                    var backgroundColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    table.Cell().Element(container => DataCellStyle(container, backgroundColor))
                                        .Text($"#{item.ImportId}")
                                        .Style(vietnameseFont.SemiBold().FontSize(9).FontColor(Colors.Blue.Darken2));

                                    table.Cell().Element(container => DataCellStyle(container, backgroundColor))
                                        .Text(item.SupplierName ?? "Chưa xác định")
                                        .Style(vietnameseFont.FontSize(9));

                                    table.Cell().Element(container => DataCellStyle(container, backgroundColor))
                                        .AlignRight()
                                        .Text($"{item.TotalPrice:N0} đ")
                                        .Style(vietnameseFont.SemiBold().FontSize(9).FontColor(Colors.Green.Darken2));

                                    table.Cell().Element(container => DataCellStyle(container, backgroundColor))
                                        .AlignCenter()
                                        .Text(item.TotalItems.ToString())
                                        .Style(vietnameseFont.FontSize(9));

                                    table.Cell().Element(container => DataCellStyle(container, backgroundColor))
                                        .Text(item.Note ?? "")
                                        .Style(vietnameseFont.FontSize(8).FontColor(Colors.Grey.Darken1));

                                    table.Cell().Element(container => DataCellStyle(container, backgroundColor))
                                        .Text(item.UserName ?? "N/A")
                                        .Style(vietnameseFont.FontSize(9));

                                    table.Cell().Element(container => DataCellStyle(container, backgroundColor))
                                        .AlignCenter()
                                        .Text(item.CreatedAt.ToString("dd/MM/yyyy HH:mm"))
                                        .Style(vietnameseFont.FontSize(8));

                                    table.Cell().Element(container => DataCellStyle(container, backgroundColor))
                                        .AlignCenter()
                                        .Text(item.ReferenceType ?? "N/A")
                                        .Style(vietnameseFont.FontSize(8));

                                    rowIndex++;
                                }

                                IContainer DataCellStyle(IContainer container, string backgroundColor)
                                {
                                    return container
                                        .Background(backgroundColor)
                                        .BorderBottom(0.5f)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(6);
                                }
                            });

                        // FOOTER - Minimalist design
                        page.Footer()
                            .Height(40)
                            .Padding(10)
                            .Column(footerColumn =>
                            {
                                footerColumn.Item().LineHorizontal(0.5f).LineColor(Colors.Blue.Darken3);
                                footerColumn.Item().PaddingTop(5);

                                footerColumn.Item().Row(footerRow =>
                                {
                                    footerRow.RelativeItem().Text("© T-SOLEKING MANAGEMENT SYSTEM")
                                        .Style(vietnameseFont.SemiBold().FontSize(8).FontColor(Colors.Blue.Darken3));

                                    footerRow.RelativeItem().AlignRight().Text(text =>
                                    {
                                        text.Span("Trang ").Style(vietnameseFont.FontSize(8).FontColor(Colors.Grey.Darken1));
                                        text.CurrentPageNumber().Style(vietnameseFont.SemiBold().FontSize(9).FontColor(Colors.Blue.Darken3));
                                        text.Span("/").Style(vietnameseFont.FontSize(8).FontColor(Colors.Grey.Darken1));
                                        text.TotalPages().Style(vietnameseFont.SemiBold().FontSize(9).FontColor(Colors.Blue.Darken3));
                                    });
                                });
                            });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo PDF báo cáo với QuestPDF: {ErrorMessage}", ex.Message);
                throw new Exception($"Không thể tạo file PDF báo cáo: {ex.Message}", ex);
            }
        }
    }
}
