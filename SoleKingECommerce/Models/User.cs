using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public enum UserType
    {
        Customer,
        Employee
    }

    public class User : IdentityUser
    {
        // Thông tin chung
        public string? Avatar { get; set; }

        [MaxLength(150)]
        public string? Name { get; set; }
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
        public UserType UserType { get; set; }

        // Dành riêng cho Employee
        public string? Position { get; set; }

        // Navigation cho Customer
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Wishlist>? Wishlists { get; set; }
        public ICollection<ProductReview>? Reviews { get; set; }
        public ICollection<Cart>? Carts { get; set; } = new List<Cart>();

        // Navigation cho Employee
        public ICollection<Import>? Imports { get; set; }
    }
}
