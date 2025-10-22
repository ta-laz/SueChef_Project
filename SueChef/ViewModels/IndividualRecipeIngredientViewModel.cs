
namespace SueChef.ViewModels
{
    public class IndividualRecipeIngredientViewModel //Seperate because it needs to great each ingredient as seperate. 
    {
        public string Name { get; set; }
        public float? Calories { get; set; }
        public float? Carbs { get; set; }
        public float? Protein { get; set; }
        public float? Fats { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }

    }
}