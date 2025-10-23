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
        var recipe = await _db.Recipes //Pulling recipes and FKs from the db
                                .Include(r => r.Chef)
                                .Include(r => r.RecipeIngredients)
                                    .ThenInclude(ri => ri.Ingredient)
                                .FirstOrDefaultAsync(r => r.Id == id);

        var ratings = await _db.Ratings.Where(r => r.RecipeId == id).ToListAsync(); //Pulling the ratings from the db
        double? avgRatings = ratings.Any() ? ratings.Average(r => r.Stars) : 0; //Calculates the average for when we need it later if no ratings default to 0 
        int TotalRatings = ratings.Count(); //Counts total ratings 

        int? currentUserId = HttpContext.Session.GetInt32("user_id");
        var userRating = currentUserId != null //If the user is logged in - get the rating from the database, otherwise use the default  
                                        ? await _db.Ratings
                                            .Where(r => r.RecipeId == id && r.UserId == currentUserId.Value)
                                            .Select(r => r.Stars)
                                            .FirstOrDefaultAsync()
                                         : 0; //The default 


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
            AverageRating = avgRatings,
            UserRating = userRating //Passing all the ratings and user rating in the controller so we know if they have or haven't rated. 
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
        int? currentUserId = HttpContext.Session.GetInt32("user_id"); 

        if (currentUserId == null) //If user is NOT logged in re-direct to log-in page - Do we want it to pop up in an error before re-directing? 
        {
            return RedirectToAction("New", "Sessions");
        }
        var existingRating = await _db.Ratings
                                            .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == currentUserId.Value); //Pulling the rating of the user that is logged in (if they're logged in)

        if (existingRating != null) //If the rating exists and they rate it again it will update and display the following message.
        {
            existingRating.Stars = rating;
            _db.Ratings.Update(existingRating);
            TempData["SuccessMessage"] = "Your rating has been updated!";

        }
        else //otherwise 
        {
            var newRating = new Rating
            {
                RecipeId = recipeId,
                Stars = rating,
                UserId = currentUserId,
            };
            _db.Ratings.Add(newRating);
            TempData["SuccessMessage"] = "Thanks for rating!"; //first time rating 

        }

        var Recipe = await _db.Recipes.FindAsync(recipeId);
        if (Recipe == null) return NotFound();

        await _db.SaveChangesAsync();
        return RedirectToAction("Index", new { id = recipeId }); //Re load the page 
    }

}