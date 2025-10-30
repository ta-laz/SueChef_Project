namespace SueChef.ViewModels
{

    public class FeaturedRecipeViewModel
    {
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? RecipePicturePath { get; set; }
        public string? Category { get; set; }
    }
}