namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MealPlan
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User User { get; set; } = null!;
    public string? MealPlanTitle { get; set; }
    public DateOnly? CreatedOn { get; set; }
    public DateOnly? UpdatedOn { get; set; }
    public ICollection<MealPlanRecipe> MealPlanRecipes { get; set; } = new List<MealPlanRecipe>();

}