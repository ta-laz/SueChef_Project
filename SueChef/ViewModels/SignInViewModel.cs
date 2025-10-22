using System.ComponentModel.DataAnnotations;
namespace SueChef.ViewModels;

public class SignInViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}

