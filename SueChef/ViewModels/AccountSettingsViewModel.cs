namespace SueChef.ViewModels;

public class AccountSettingsViewModel
{
    public int Id { get; set; }
    public string? CurrentUserName { get; set; }
    public string? CurrentEmail { get; set; }
    public DateOnly? DateJoined { get; set; }

    public string? SuccessMessage { get; set; }
    public string? DeleteError { get; set; }
}
