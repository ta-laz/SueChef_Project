namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;


public class Rating
{
    [Key]
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int? Stars { get; set; }
    public int? UserId { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;


}