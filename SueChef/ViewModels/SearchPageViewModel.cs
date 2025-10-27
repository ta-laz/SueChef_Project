namespace SueChef.ViewModels
{
    public class SearchPageViewModel
    {
        public string? SearchQuery { get; set; }
        public List<RecipeCardViewModel> Recipes { get; set; } = new();
        public List<string> AllIngredients { get; set; } = new();
        public List<string> AllCategories { get; set; } = new();
        public List<string> AllChefs { get; set; } = new();
    }
}
