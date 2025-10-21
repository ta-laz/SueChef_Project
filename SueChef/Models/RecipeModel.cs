namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Recipe
{
    [Key]
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Serving { get; set; }
    public int DifficultyLevel { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsDairyFree { get; set; }
    public string? Category { get; set; }
    public int ChefId { get; set; }

    [ForeignKey("ChefId")]
    public Chef? Chef { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RecipePicturePath { get; set; }

}