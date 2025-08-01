namespace SoleKingECommerce.Models
{
    public class ImportReference
    {
        public int Id { get; set; }

        public int FromImportId { get; set; }
        public int ToImportId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Import FromImport { get; set; }
        public Import ToImport { get; set; }

        public ICollection<ImportReferenceItem> Items { get; set; }
    }
}
