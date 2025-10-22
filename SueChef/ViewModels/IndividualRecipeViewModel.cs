namespace SueChef.ViewModels
{
    public class IndividualRecipeViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int DifficultyLevel { get; set; }
        public string? RecipePicturePath { get; set; }
        public string? ChefName { get; set; }
        public List<IndividualRecipeIngredientViewModel>? Ingredients { get; set; }
    }
}