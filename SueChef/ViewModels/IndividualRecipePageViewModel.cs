namespace SueChef.ViewModels;

public class IndividualRecipePageViewModel
{
    public IndividualRecipeViewModel IndividualRecipe { get; set; }
    public bool IsFavourited { get; set; }
    public List<MealPlanViewModel>? UserMealPlans { get; set; } = new();
    public bool IsLoggedIn { get; set; }

    //public CommentingViewModel commenting { get; set; } = new CommentingViewModel();
    public List<CommentingViewModel> CommentsList { get; set; } =  new();
    
}