namespace SueChef.ViewModels;

public class MealPlansPageViewModel
{
    public IEnumerable<MealPlanViewModel>? MealPlans { get; set; } = new List<MealPlanViewModel>();
    public MealPlanViewModel? MealPlanViewModel { get; set; }
}