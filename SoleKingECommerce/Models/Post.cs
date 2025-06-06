using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public string? Thumbnail { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}