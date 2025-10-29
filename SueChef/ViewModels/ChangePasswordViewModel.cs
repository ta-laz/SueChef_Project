using System.ComponentModel.DataAnnotations;
namespace SueChef.ViewModels;

public class ChangePasswordViewModel
{
    [Required]
    public int Id { get; set; }

    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$",
        ErrorMessage = "Password must be ≥ 8 characters and include an uppercase letter and a special character.")]
    public string NewPassword { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = "";
}


