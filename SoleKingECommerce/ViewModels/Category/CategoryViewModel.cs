using System.ComponentModel.DataAnnotations;
using SoleKingECommerce.Models;

namespace SoleKingECommerce.ViewModels.Category
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "{0} không được để trống.")]
        [MaxLength(100, ErrorMessage = "{0} không được vượt quá 100 kí tự.")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; } = null;

        [Display(Name = "Danh mục cha")]
        public int? ParentId { get; set; }

        public int? ProductCount { get; set; } = 0;

        public List<CategoryViewModel> ParentCategories { get; set; } = new List<CategoryViewModel>();

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }
    }
}
