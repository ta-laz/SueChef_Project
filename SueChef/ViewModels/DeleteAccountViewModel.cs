using System.ComponentModel.DataAnnotations;

namespace SueChef.ViewModels
{
    public class DeleteAccountViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required, DataType(DataType.Password)]
        public string ConfirmDeletePassword { get; set; } = "";
    }
}
