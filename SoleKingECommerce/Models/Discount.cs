using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Discount
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public double PercentDiscount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ICollection<ProductDiscount>? ProductDiscounts { get; set; } = new List<ProductDiscount>();

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}