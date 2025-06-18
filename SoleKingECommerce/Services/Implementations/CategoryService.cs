using SoleKingECommerce.Helpers;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.Services.Interfaces;
using SoleKingECommerce.ViewModels.Category;

namespace SoleKingECommerce.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllWithProductsAsync();
        }

        public async Task<PaginatedList<Category>> GetPaginatedCategoriesAsync(string searchString, int? parentId, int pageIndex = 1, int pageSize = 10)
        {
            var query = _categoryRepository.GetQueryableWithProducts();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Name.Contains(searchString) ||
                        (c.Description != null && c.Description.Contains(searchString)));
            }

            if (parentId.HasValue)
            {
                query = query.Where(c => c.ParentId == parentId);
            }

            return await PaginatedList<Category>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Category>> GetParentCategoriesForFilterAsync()
        {
            return await _categoryRepository.GetParentCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdWithProductsAsync(id);
        }

        public async Task<CategoryViewModel> GetCategoryForCreateAsync()
        {
            var parentCategories = await _categoryRepository.GetParentCategoriesAsync();

            return new CategoryViewModel
            {
                ParentCategories = parentCategories
                    .Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ParentId = c.ParentId,
                        CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                        UpdatedAt = c.UpdatedAt ?? DateTime.MinValue
                    }).ToList()
            };
        }

        public async Task<CategoryViewModel> GetCategoryForEditAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            var parentCategories = await _categoryRepository.GetParentCategoriesAsync(id);

            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
                CreatedAt = category.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = category.UpdatedAt ?? DateTime.MinValue,
                ParentCategories = parentCategories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                    UpdatedAt = c.UpdatedAt ?? DateTime.MinValue
                }).ToList()
            };
        }

        public async Task<IEnumerable<object>> GetLeafCategoriesWithParentNamesAsync()
        {
            var allCategories = await GetAllCategoriesAsync();

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

        public async Task CreateCategoryAsync(CategoryViewModel model)
        {
            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                ParentId = model.ParentId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _categoryRepository.AddAsync(category);
        }

        public async Task UpdateCategoryAsync(CategoryViewModel model)
        {
            var category = await _categoryRepository.GetByIdAsync(model.Id);
            if (category == null)
            {
                throw new ArgumentException("Category not found");
            }

            category.Name = model.Name;
            category.Description = model.Description;
            category.ParentId = model.ParentId;
            category.UpdatedAt = DateTime.Now;

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new ArgumentException("Category not found");
            }

            if (await _categoryRepository.HasProductsAsync(id))
            {
                throw new InvalidOperationException("Cannot delete category with existing products");
            }

            await _categoryRepository.DeleteAsync(category);
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _categoryRepository.ExistsAsync(id);
        }
    }
}
