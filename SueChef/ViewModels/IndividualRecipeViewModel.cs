namespace SueChef.ViewModels
{
    public class IndividualRecipeViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Method { get; set; }
        public int DifficultyLevel { get; set; }
        public string? RecipePicturePath { get; set; }
        public string? ChefName { get; set; }
        public int? CookTime {get; set; }
        public int? PrepTime {get; set;}
        public List<IndividualRecipeIngredientViewModel>? Ingredients { get; set; }
        public int Servings {get; set;}
        public decimal CaloriesPerServing {get; set;}
        public decimal ProteinPerServing { get; set; }
        public decimal CarbsPerServing { get; set; }
        public decimal FatsPerServing { get; set; }
    }
}