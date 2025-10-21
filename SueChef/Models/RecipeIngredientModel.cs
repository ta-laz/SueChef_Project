namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class RecipeIngredient
{
    [Key]
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }

    [ForeignKey("RecipeId")]
    public Recipe Recipe { get; set; }

    [ForeignKey("IngredientId")]

    public Ingredient Ingredient { get; set; }
}