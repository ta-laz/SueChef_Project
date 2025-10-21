namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;

public class Chef
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public ICollection<Recipe> Recipe { get; set; }
}