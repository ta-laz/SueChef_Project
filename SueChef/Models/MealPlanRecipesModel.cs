namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MealPlanRecipe
{
    public int Id { get; set; }
    public string? MealPlanId { get; set; }
    public MealPlan MealPlan { get; set; } = null!;

    public string? RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}