namespace SueChef.ViewModels;

public class SingleMealPlanPageViewModel
{
    public RecipeCardViewModel? RecipeCardViewModel { get; set; }

    public MealPlanRecipeViewModel? MealPlanRecipeViewModel { get; set; }

    public IEnumerable<IndividualRecipeIngredientViewModel>? IngredientsList { get; set; } = new List<IndividualRecipeIngredientViewModel>();

    public IEnumerable<RecipeCardViewModel>? RecipesList { get; set; } = new List<RecipeCardViewModel>();

    public MealPlanViewModel? MealPlan { get; set; }
}