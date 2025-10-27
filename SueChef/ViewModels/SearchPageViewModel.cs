namespace SueChef.ViewModels
{
    public class SearchPageViewModel
    {
        public string? SearchQuery { get; set; }
        public List<RecipeCardViewModel> Recipes { get; set; } = new();
    }
}
