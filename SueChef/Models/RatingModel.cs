namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;


public class Rating
{
    [Key]
    public int RatingId { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipes { get; set; } = null!;

    public int? stars { get; set; }
    public int UserId { get; set; }

    public User Users { get; set; } = null!;

    public DateTime CreatedOn { get; set; }


}