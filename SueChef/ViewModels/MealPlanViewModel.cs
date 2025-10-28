namespace SueChef.ViewModels;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using SueChef.Models;

public class MealPlanViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required"), StringLength(200, ErrorMessage = "Don't be ridiculous, choose a shorter title")]
    public string? MealPlanTitle { get; set; }
    public int? RecipeCount { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public List<string>? RecipePicturePaths { get; set; } = new();

}