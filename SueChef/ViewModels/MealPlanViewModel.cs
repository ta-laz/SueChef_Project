namespace SueChef.ViewModels;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

public class MealPlanViewModel
{
    public int Id { get; set; }
    public string? MealPlanTitle { get; set; }
    public int RecipeCount { get; set; }
    public DateOnly? UpdatedOn { get; set; }

}