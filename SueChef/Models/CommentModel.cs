namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;

public class Comment
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User {get; set;}
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public string? Content { get; set; }
    public DateTime CreatedOn { get; set; }

}