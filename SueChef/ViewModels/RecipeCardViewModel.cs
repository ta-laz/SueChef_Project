namespace SueChef.ViewModels
{

    public class RecipeCardViewModel
    {
        public int Id { get; set; } // To link to the full recipe
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string ShortDescription { get; set; }
    }
}