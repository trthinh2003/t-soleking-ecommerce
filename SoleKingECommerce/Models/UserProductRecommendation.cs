using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class UserProductRecommendation
    {
        [Key]
        public int Id { get; set; }

        public string ForUserId { get; set; }
        public User ForUser { get; set; }

        public int RecommendedProductId { get; set; }
        public Product RecommendedProduct { get; set; }

        public decimal Score { get; set; }
        public int SimilarUsersCount { get; set; }
        public string SimilarUserIds { get; set; }

        public DateTime GeneratedAt { get; set; }
        public DateTime ExpiresAt { get; set; } 
    }
}
