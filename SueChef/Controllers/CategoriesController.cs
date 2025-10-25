using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;

namespace SueChef.Controllers;

public class CategoriesController : Controller
{
    private readonly ILogger<CategoriesController> _logger;
    private readonly SueChefDbContext _db;

    public CategoriesController(ILogger<CategoriesController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index(string category)
    {
        // redirect to home page if no category is given
        if (string.IsNullOrEmpty(category))
        {
            return RedirectToAction("Index", "Home");
        }

        IQueryable<Recipe> query = _db.Recipes;

        string pageTitle = "";

        // if the string provided is vegetarian, use the query to find all vegetarian recipes, set the pageTitle to vegetarian recipes
        // if not try the next case
        switch (category.ToLower())
        {
            case "vegetarian":
                query = query.Where(r => r.IsVegetarian);
                pageTitle = "Vegetarian Recipes";
                break;
            case "dairyfree":
                query = query.Where(r => r.IsDairyFree);
                pageTitle = "Dairy-Free Recipes";
                break;
            case "quick":
                query = query.Where(r => (r.PrepTime + r.CookTime) < 60);
                pageTitle = "Quick Meals";
                break;
            case "easy":
                query = query.Where(r => r.DifficultyLevel == 1);
                pageTitle = "Easy Recipes";
                break;
            case "medium":
                query = query.Where(r => r.DifficultyLevel == 2);
                pageTitle = "Medium Difficulty Recipes";
                break;
            case "hard":
                query = query.Where(r => r.DifficultyLevel == 3);
                pageTitle = "Hard Recipes";
                break;
            default:
                pageTitle = "All Recipes";
                break;
        }

        // convert the recipes from the query into new recipecardviewmodel objects
        var recipes = await query.Select(r => new RecipeCardViewModel
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            RecipePicturePath = r.RecipePicturePath,
            IsVegetarian = r.IsVegetarian,
            IsDairyFree = r.IsDairyFree,
            DifficultyLevel = r.DifficultyLevel,
            PrepTime = r.PrepTime,
            CookTime = r.CookTime,

            RatingCount = _db.Ratings.Count(rt => rt.RecipeId == r.Id && rt.Stars.HasValue),
            AverageRating = _db.Ratings
            .Where(rt => rt.RecipeId == r.Id && rt.Stars.HasValue)
            .Average(rt => (double?)rt.Stars) ?? 0
        })
        .ToListAsync();

        // make the title the pageTitle and the recipes the newly created list of recipecardviewmodel objects
        var viewModel = new CategoryPageViewModel
        {
            Title = pageTitle,
            Recipes = recipes
        };

        return View(viewModel);
    }

}