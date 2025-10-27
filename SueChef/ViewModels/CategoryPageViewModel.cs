namespace SueChef.ViewModels;

public class CategoryPageViewModel
{
    public string Title { get; set; }
    public IEnumerable<RecipeCardViewModel> Recipes { get; set; } = new List<RecipeCardViewModel>();
    
}