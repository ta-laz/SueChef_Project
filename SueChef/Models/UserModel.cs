namespace SueChef.Models;

using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int Id { get; set; }
    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }
    public DateOnly? DateJoined { get; set; }
    public DateOnly? DOB { get; set; }

    public ICollection<MealPlan> MealPlan { get; set; } = new List<MealPlan>();

}