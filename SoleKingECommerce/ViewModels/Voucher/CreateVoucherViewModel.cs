using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Voucher
{
    public class CreateVoucherViewModel
    {
        [Required(ErrorMessage = "Mã giảm giá không được để trống")]
        [StringLength(50, ErrorMessage = "Mã giảm giá không được vượt quá 50 ký tự")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Phần trăm giảm giá không được để trống")]
        [Range(0.01, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0.01% đến 100%")]
        public decimal DiscountPercent { get; set; }

        [Required(ErrorMessage = "Giảm giá tối đa không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giảm giá tối đa phải lớn hơn 0")]
        public decimal MaxDiscount { get; set; }

        [Required(ErrorMessage = "Đơn hàng tối thiểu không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal MinOrderAmount { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Giới hạn sử dụng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0")]
        public int UsageLimit { get; set; }
    }
}
