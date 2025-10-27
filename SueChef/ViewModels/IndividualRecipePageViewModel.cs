namespace SueChef.ViewModels;

public class IndividualRecipePageViewModel
{
    public IndividualRecipeViewModel IndividualRecipe { get; set; }

    //public CommentingViewModel commenting { get; set; } = new CommentingViewModel();
    public List<CommentingViewModel> CommentsList { get; set; } =  new();
}