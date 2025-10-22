namespace SueChef.ViewModels;

public class HomePageViewModel
{
    public IEnumerable<RecipeCardViewModel> RecipeCards { get; set; } = new List<RecipeCardViewModel>();
    public FeaturedRecipeViewModel FeaturedRecipe { get; set; }
}