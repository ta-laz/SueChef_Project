namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;

public class ShoppingList
{
    public int ShoppingListId { get; set; }

    public int UserId { get; set; }

    public User Users { get; set; } = null!;
    
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    public decimal? Quantity { get; set; }

    public int? unit { get; set; }

    public bool Purchased { get; set; } = false;

}