namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ShoppingList
{
    public int Id { get; set; }
    public int UserId {get; set;}
    public User? User { get; set; }

    public string? Category { get; set; }
    public string? IngredientName { get; set; } = null;
    public decimal? Quantity { get; set; } = null;
    public string? Unit { get; set; } = null;

    public string? Additional { get; set; } = null;
    public string? AdditionalQuantity { get; set; } = null;
    public bool IsPurchased { get; set; } = false;  

}