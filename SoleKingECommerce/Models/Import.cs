using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Import
    {
        [Key]
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Note { get; set; }

        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        public ICollection<ImportItem>? Items { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}