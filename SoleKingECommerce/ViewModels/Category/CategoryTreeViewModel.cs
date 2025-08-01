namespace SoleKingECommerce.ViewModels.Category
{
    public class CategoryTreeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<CategoryTreeViewModel> Children { get; set; } = new List<CategoryTreeViewModel>();
    }
}
