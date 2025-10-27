namespace SueChef.ViewModels;

using SueChef.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MealPlanRecipeViewModel
{
    public int Id { get; set; }
    public int? MealPlanId { get; set; }
    public MealPlan MealPlan { get; set; } = null!;

    public int? RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}