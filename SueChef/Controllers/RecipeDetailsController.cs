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

    // [Route("/Recipe/{id}")]
    // [HttpGet]
    // public async Task<IActionResult> Index(int id)
    // {
    //     var recipe = await _db.Recipes.Where(r => r.Id == id)
    //                                 .Include(r => r.Chef)
    //                                 .Include(r => r.RecipeIngredients)
    //                                     .ThenInclude(ri => ri.Ingredient)
    //                                 .Select(r => new IndividualRecipeViewModel
    //                                 {
    //                                     Id = r.Id,
    //                                     Title = r.Title,
    //                                     Description = r.Description,
    //                                     DifficultyLevel = r.DifficultyLevel,
    //                                     RecipePicturePath = r.RecipePicturePath,
    //                                     ChefName = r.Chef.Name,
    //                                     //Ingredients = recipe.RecipeIngredients.Select(ri => new IngredientViewModel
    //                                     // {
    //                                     //     Name = ri.Ingredient.Name,


    //                                     // })

    //                                 }).FirstOrDefaultAsync();
                                    

    //     if (recipe == null)
    //     {
    //         return NotFound();
    //     }

    //     return View(recipe);
    // }
//}




    [Route("/Recipe/{id}")]
    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        var recipe = await _db.Recipes
                                .Include(r => r.Chef)
                                .Include(r => r.RecipeIngredients)
                                    .ThenInclude(ri => ri.Ingredient)
                                .FirstOrDefaultAsync(r => r.Id == id);
        if (recipe == null)
            return NotFound();
        var viewModel = new IndividualRecipeViewModel
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            Method = recipe.Method,
            DifficultyLevel = recipe.DifficultyLevel,
            RecipePicturePath = recipe.RecipePicturePath,
            ChefName = recipe.Chef?.Name,
            Ingredients = recipe.RecipeIngredients.Select(ri => new IndividualRecipeIngredientViewModel
            {
                Name = ri.Ingredient.Name,
                Calories = ri.Ingredient.Calories,
                Carbs = ri.Ingredient.Carbs,
                Protein = ri.Ingredient.Protein,
                Fats = ri.Ingredient.Fat,
                //Quantity = ri.Quantity,
                //Unit = ri.Unit
            }).ToList()
        };
        return View(viewModel);
    }
}