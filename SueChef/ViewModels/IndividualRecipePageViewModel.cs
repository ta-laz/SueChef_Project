namespace SueChef.ViewModels;

public class IndividualRecipePageViewModel
{
    public IndividualRecipeViewModel IndividualRecipe { get; set; }
    public bool IsFavourited { get; set; }
    public List<MealPlanViewModel>? UserMealPlans { get; set; } = new();
    public bool IsSignedIn { get; set; }
}