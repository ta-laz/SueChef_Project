namespace SueChef.ViewModels;

public class SingleMealPlanPageViewModel
{
    public RecipeCardViewModel? RecipeCardViewModel { get; set; }
    
    public MealPlanRecipeViewModel? MealPlanRecipeViewModel { get; set; }

    public IEnumerable<IndividualRecipeIngredientViewModel>? IngredientsList { get; set; }
}