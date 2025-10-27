namespace SueChef.ViewModels
{

    public class RecipeCardViewModel
    {
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DifficultyLevel { get; set; }
        public bool IsVegetarian { get; set; }
        public bool IsDairyFree { get; set; }
        public string? RecipePicturePath { get; set; }
        public string? Category { get; set; }
        public int PrepTime { get; set; }
        public int CookTime { get; set; }
        public double? AverageRating { get; set; }
        public int RatingCount { get; set; }

        public string TotalTimeDisplay
        {
            get
            {
                int totalTime = CookTime + PrepTime;
                int hours = totalTime / 60;
                int minutes = totalTime % 60;

                // just return number of minutes + mins
                if (hours == 0)
                    return $"{minutes} mins";
                // if minutes are 0 then don't show them
                // return hr by default
                // if hours variable is more than 1 then include s after the hr
                if (minutes == 0)
                    return $"{hours} hr{(hours > 1 ? "s" : "")}";
                // same as above but include minutes at the end
                return $"{hours} hr{(hours > 1 ? "s" : "")} {minutes} mins";
            }
        }
        public int? MealPlanRecipeId { get; set; }
        public List<IndividualRecipeIngredientViewModel>? Ingredients { get; set; }    
        
    }
}