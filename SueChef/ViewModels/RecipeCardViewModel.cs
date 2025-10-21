namespace SueChef.ViewModels
{

    public class RecipeCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? RecipePicturePath { get; set; }
        public string? Category { get; set; }
        public int DifficultyLevel { get; set; }
    }
}