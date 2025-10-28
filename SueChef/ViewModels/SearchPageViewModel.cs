namespace SueChef.ViewModels
{
    public class SearchPageViewModel
    {
        public string? SearchQuery { get; set; }
        public List<RecipeCardViewModel> Recipes { get; set; } = new();
        public List<string> AllIngredients { get; set; } = new();
        public List<string> AllCategories { get; set; } = new();
        public List<string> AllChefs { get; set; } = new();
        public bool HasSearch { get; set; }
        public string? SearchCategory { get; set; }
        public string? SearchChef { get; set; }
        public List<string>? SelectedIngredients { get; set; }
        public List<string>? DietarySelections { get; set; }
        public int? Difficulty { get; set; }
        public string? DurationBucket { get; set; }
    }
}
