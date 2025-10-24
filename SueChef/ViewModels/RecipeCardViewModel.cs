namespace SueChef.ViewModels
{

    public class RecipeCardViewModel
    {
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DifficultyLevel { get; set; }
        public bool IsVegetarian { get; set; }
        public bool IsDairyFree { get; set; }
        public string? RecipePicturePath { get; set; }
        public string? Category { get; set; }
        public int? MealPlanRecipeId { get; set; }
        public List<IndividualRecipeIngredientViewModel>? Ingredients { get; set; }    
        
    }
}