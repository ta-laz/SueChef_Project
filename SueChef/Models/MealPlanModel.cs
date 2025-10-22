namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;

public class MealPlan
{
    [Key]
    public int MealPlanId { get; set; }
    
    public int UserId { get; set; }

    public User Users { get; set; } = null!;
    
    public int RecipeId { get; set; }
    public Recipe Recipes { get; set; } = null!;

    public string? name { get; set; }

    public DateTime UpdatedOn { get; set; } = DateTime.Now;

    public bool? IsActive { get; set; }
    
    public DateTime CreatedOn { get; set; }

}