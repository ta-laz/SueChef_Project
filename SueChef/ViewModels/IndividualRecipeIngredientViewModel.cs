
namespace SueChef.ViewModels
{
    public class IndividualRecipeIngredientViewModel //Seperate because it needs to great each ingredient as seperate. 
    {
        public string Name { get; set; }
        public string? Calories { get; set; }
        public string? Carbs { get; set; }
        public string? Protein { get; set; }
        public string? Fats { get; set; }

    }
}