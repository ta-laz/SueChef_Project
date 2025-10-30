using SueChef.Models;
namespace SueChef.ViewModels;


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ShoppingListViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int? RecipeIngredientId { get; set; } = null;
    public RecipeIngredient RecipeIngredient { get; set; } = null!;
    public bool IsPurchased { get; set; } = false;

    public int? Servings { get; set; }
    public string? Additional { get; set; } = null;
    public string? AdditionalQuantity { get; set; } = null;

}