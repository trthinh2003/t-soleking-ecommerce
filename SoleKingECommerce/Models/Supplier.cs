using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(150)]
        public string? Name { get; set; }

        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        public ICollection<Import>? Imports { get; set; }
    }
}