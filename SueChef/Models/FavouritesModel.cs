namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Favourite
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User User { get; set; } = null!;
    public int? RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int? Servings { get; set; } = 4;
}