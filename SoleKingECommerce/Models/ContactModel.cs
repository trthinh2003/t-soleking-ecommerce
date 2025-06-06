using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class ContactModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string Phone { get; set; }

        public string Subject { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Message { get; set; }
    }
}
