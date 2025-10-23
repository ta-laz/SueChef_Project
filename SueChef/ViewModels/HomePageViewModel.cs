namespace SueChef.ViewModels;

public class HomePageViewModel
{
    public IEnumerable<RecipeCardViewModel> RecipeCards { get; set; } = new List<RecipeCardViewModel>();
    public FeaturedRecipeViewModel FeaturedRecipe { get; set; }
    
    // Updated properties for distinct carousels
    public RecipeCarouselViewModel AllRecipesCarousel { get; set; } 
    public RecipeCarouselViewModel VegetarianRecipesCarousel { get; set; }
    public RecipeCarouselViewModel DairyFreeRecipesCarousel { get; set; }
}