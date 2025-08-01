using Microsoft.AspNetCore.Mvc;
using SoleKingECommerce.Services.Interfaces;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly ICategoryService _categoryService;

    public CategoryMenuViewComponent(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _categoryService.GetCachedCategoryTreeAsync();
        return View(categories);
    }
}