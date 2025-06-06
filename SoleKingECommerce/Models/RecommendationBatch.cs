using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.Models
{
    public class RecommendationBatch
    {
        [Key]
        public int Id { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } // "Running", "Completed", "Failed"

        public int TotalUsers { get; set; }
        public int ProcessedUsers { get; set; }
        public int GeneratedRecommendations { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
