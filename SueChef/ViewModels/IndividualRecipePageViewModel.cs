namespace SueChef.ViewModels;

public class IndividualRecipePageViewModel
{
    public IndividualRecipeViewModel IndividualRecipe { get; set; }
    public bool IsFavourited { get; set; }
    public List<MealPlanViewModel>? UserMealPlans { get; set; } = new();
    public bool IsLoggedIn { get; set; }
    public List<int> MealPlanIdsWithRecipe { get; set; } = new List<int>();
}