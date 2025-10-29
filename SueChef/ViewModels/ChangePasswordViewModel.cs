using System.ComponentModel.DataAnnotations;
namespace SueChef.ViewModels;

public class ChangePasswordViewModel
{
    [Required]
    public int Id { get; set; }

    [Required, DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$",
        ErrorMessage = "Password must be ≥ 8 characters and include an uppercase letter and a special character.")]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm New Password")]
    public string ConfirmNewPassword { get; set; } = "";
}


