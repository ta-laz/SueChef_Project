namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MealPlanRecipe
{
    public int Id { get; set; }
    public int? MealPlanId { get; set; }
    public MealPlan MealPlan { get; set; } = null!;

    public int? RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;  // NEW FLAG for soft-delete

}