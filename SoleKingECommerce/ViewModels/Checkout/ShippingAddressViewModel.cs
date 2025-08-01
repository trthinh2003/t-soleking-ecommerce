using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Checkout
{
    public class ShippingAddressViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string StreetAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành phố")]
        public string City { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quận/huyện")]
        public string District { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phường/xã")]
        public string Ward { get; set; }

        public string Notes { get; set; }
    }
}
