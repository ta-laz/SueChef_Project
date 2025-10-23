namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Recipe
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Method { get; set; }
    public int DifficultyLevel { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsDairyFree { get; set; }
    public string? Category { get; set; }
    public int PrepTime { get; set; }
    public int CookTime { get; set; }

    public int ChefId { get; set; }
    public Chef? Chef { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? RecipePicturePath { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<MealPlanRecipe> MealPlanRecipes { get; set; } = new List<MealPlanRecipe>();

}