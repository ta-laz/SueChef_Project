namespace SueChef.ViewModels;

using SueChef.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MealPlanRecipeServingViewModel
{
    public int? RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int Servings { get; set; }
}