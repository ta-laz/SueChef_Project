namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;

<<<<<<< HEAD

public class Rating
{
    [Key]
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int? Stars { get; set; }
    public int? UserId { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
=======
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
>>>>>>> main


}