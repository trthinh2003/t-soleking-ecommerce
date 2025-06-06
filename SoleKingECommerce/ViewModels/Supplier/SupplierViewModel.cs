using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Supplier
{
    public class SupplierViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên nhà cung cấp")]
        [MaxLength(150, ErrorMessage = "{0} phải dưới 150 ký tự.")]
        [Required(ErrorMessage = "{0} không được để trống.")]
        public string? Name { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "{0} không đúng định dạng email.")]
        [MaxLength(100, ErrorMessage = "{0} phải dưới 100 ký tự")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "{0} phải bắt đầu bằng 0 và có 10 chữ số")]
        [MaxLength(15, ErrorMessage = "{0} phải dưới 15 ký tự")]
        public string? Phone { get; set; }
    }
}
