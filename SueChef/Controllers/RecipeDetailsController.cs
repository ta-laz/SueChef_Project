using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;


namespace SueChef.Controllers;

public class RecipeDetailsController : Controller
{
    private readonly ILogger<RecipeDetailsController> _logger;
    private readonly SueChefDbContext _db;

    public RecipeDetailsController(ILogger<RecipeDetailsController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [Route("/Recipe/{id}")]
    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        var recipe = await _db.Recipes
                                .Include(r => r.Chef)
                                .Include(r => r.RecipeIngredients)
                                    .ThenInclude(ri => ri.Ingredient)
                                .FirstOrDefaultAsync(r => r.Id == id);

        var ratings = await _db.Ratings.Where(r => r.RecipeId == id).ToListAsync();
        double? avgRatings = ratings.Any() ? ratings.Average(r => r.Stars) : 0;
        int TotalRatings = ratings.Count();


        if (recipe == null)
            return NotFound();//Return not found page if no recipe is found 

        //Calories per serving logic, Dividing calories by 100 so we have the calorie per g and then multiplying by the quantity in the recipe. 
        decimal caloriesPerServing = recipe.RecipeIngredients.Sum(ri => ((decimal)ri.Ingredient.Calories / 100m) * ri.Quantity);
        decimal proteinPerServing = recipe.RecipeIngredients.Sum(ri => ((decimal)ri.Ingredient.Protein / 100m) * ri.Quantity);
        decimal carbsPerServing = recipe.RecipeIngredients.Sum(ri => ((decimal)ri.Ingredient.Carbs / 100m) * ri.Quantity);
        decimal fatsPerServing = recipe.RecipeIngredients.Sum(ri => ((decimal)ri.Ingredient.Fat / 100m) * ri.Quantity);


        var viewModel = new IndividualRecipeViewModel //Creating the viewmodel data
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            Method = recipe.Method,
            DifficultyLevel = recipe.DifficultyLevel,
            RecipePicturePath = recipe.RecipePicturePath,
            ChefName = recipe.Chef?.Name,
            PrepTime = recipe.PrepTime,
            CookTime = recipe.CookTime,
            Ingredients = recipe.RecipeIngredients.Select(ri => new IndividualRecipeIngredientViewModel
            {
                Name = ri.Ingredient.Name,
                Calories = ri.Ingredient.Calories,
                Carbs = ri.Ingredient.Carbs,
                Protein = ri.Ingredient.Protein,
                Fats = ri.Ingredient.Fat,
                Quantity = ri.Quantity,
                Unit = ri.Unit
            }).ToList(),
            CaloriesPerServing = caloriesPerServing,//Per serving calculations passed into the var
            ProteinPerServing = proteinPerServing,
            CarbsPerServing = carbsPerServing,
            FatsPerServing = fatsPerServing,
            RatingCount = TotalRatings,
            AverageRating = avgRatings
        };
        var AllViewModels = new IndividualRecipePageViewModel
        {
            IndividualRecipe = viewModel
        };
        return View(AllViewModels);

    }
    [HttpPost]
    public async Task<IActionResult> Rate(int recipeId, int rating)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        var Recipe = await _db.Recipes.FindAsync(recipeId);
        if (Recipe == null) return NotFound();
        var Rating = new Rating
        {
            RecipeId = recipeId,
            Stars = rating,
            UserId = currentUserId,
        };
        _db.Ratings.Add(Rating);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", new { recipeId });
    }

}