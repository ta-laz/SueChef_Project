namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class RecipeIngredient
{
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
}