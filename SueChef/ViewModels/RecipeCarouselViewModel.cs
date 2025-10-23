namespace SueChef.ViewModels
{
    public class RecipeCarouselViewModel
    {
        public string Title { get; set; } = "Recipes";
        public string CarouselId { get; set; } = "carousel";
        public List<RecipeCardViewModel> Recipes { get; set; } = new();
    }
}
