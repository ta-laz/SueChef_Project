namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Ingredient
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Fat { get; set; }
    public float Carbs { get; set; }
    public string? Category { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; }

}