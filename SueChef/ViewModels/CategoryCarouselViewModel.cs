namespace SueChef.ViewModels
{
    public class CategoryCarouselViewModel
    {
        public string Title { get; set; } = "Recipes";
        public string CarouselId { get; set; } = "carousel";
        public List<CategoryCardViewModel> Categories { get; set; } = new();
    }
}
