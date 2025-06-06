namespace SoleKingECommerce.Models
{
    public class UserSimilarity
    {
        public string UserId1 { get; set; }
        public User User1 { get; set; }

        public string UserId2 { get; set; }
        public User User2 { get; set; }

        public decimal JaccardScore { get; set; } // 0.0 - 1.0
        public int CommonProducts { get; set; } // |A ∩ B|
        public int TotalUniqueProducts { get; set; } // |A ∪ B|

        public DateTime CalculatedAt { get; set; }
    }
}
