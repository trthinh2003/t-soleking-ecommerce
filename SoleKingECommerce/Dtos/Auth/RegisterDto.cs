using SoleKingECommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Dtos.Auth
{
    public class RegisterDto
    {
        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Email { get; set; }

        public UserType UserType { get; set; } = UserType.Customer;

        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }
    }
}
