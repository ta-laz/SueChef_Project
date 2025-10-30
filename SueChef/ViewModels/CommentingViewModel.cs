namespace SueChef.ViewModels
{
    public class CommentingViewModel 
    {
        public int Id { get; set; }

        public int RecipeId { get; set; }
        public string userName {get; set;}
        public string? Content { get; set; }

        public DateTime CreatedOn { get; set; }
        
    }
}