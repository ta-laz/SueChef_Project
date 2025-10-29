using System.ComponentModel.DataAnnotations;

namespace SueChef.ViewModels
{
    public class ChangeEmailViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string NewEmail { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
