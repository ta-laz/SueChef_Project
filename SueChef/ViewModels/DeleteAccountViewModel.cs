using System.ComponentModel.DataAnnotations;

namespace SueChef.ViewModels
{
    public class DeleteAccountViewModel
    {
        [Required]
        public int Id { get; set; } // user id

        [Required, DataType(DataType.Password)]
        [Display(Name = "Enter Password")]

        public string ConfirmDeletePassword { get; set; } = string.Empty;
    }
}
