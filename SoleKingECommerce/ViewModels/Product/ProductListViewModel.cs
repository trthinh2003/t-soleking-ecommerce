namespace SoleKingECommerce.ViewModels.Product
{
    public class ProductListViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public string? Tags { get; set; }
        public string? Slug { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int VariantCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
