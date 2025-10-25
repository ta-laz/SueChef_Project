namespace SueChef.ViewModels;

public class HomePageViewModel
{
    public IEnumerable<RecipeCardViewModel> RecipeCards { get; set; } = new List<RecipeCardViewModel>();
    public FeaturedRecipeViewModel TopFeaturedRecipe { get; set; }
    public FeaturedRecipeViewModel MiddleFeaturedRecipe { get; set; }
    public FeaturedRecipeViewModel BottomFeaturedRecipe { get; set; }

    public RecipeCarouselViewModel RecipeCategoriesCarousel { get; set; }
    public RecipeCarouselViewModel AllRecipesCarousel { get; set; }
    public RecipeCarouselViewModel VegetarianRecipesCarousel { get; set; }
    public RecipeCarouselViewModel DairyFreeRecipesCarousel { get; set; }
    public RecipeCarouselViewModel EasyRecipesCarousel { get; set; }
    public RecipeCarouselViewModel MediumRecipesCarousel { get; set; }
    public RecipeCarouselViewModel HardRecipesCarousel { get; set; }
    public RecipeCarouselViewModel QuickRecipesCarousel { get; set; }
    public RecipeCarouselViewModel HighlyRatedRecipesCarousel { get; set; }
    public RecipeCarouselViewModel MostRatedRecipesCarousel { get; set; }
}