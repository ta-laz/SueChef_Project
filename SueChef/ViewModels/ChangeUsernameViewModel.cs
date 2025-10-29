using System.ComponentModel.DataAnnotations;

namespace SueChef.ViewModels
{
    public class ChangeUsernameViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(30, MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 30 characters.")]
        public string NewUserName { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
