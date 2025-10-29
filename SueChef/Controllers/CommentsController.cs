using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;

namespace SueChef.Controllers;

public class CommentController : Controller
{
    private readonly SueChefDbContext _db;


    public CommentController(ILogger<CommentController> logger, SueChefDbContext db)
    {
        _db = db;
    }

    [Route("/recipe/{recipeId}/comments", Name = "commentingOnRecipe")]
    [HttpPost]
    public async Task<IActionResult> comments(int recipeId, string content)
    {
        int? currentUserId = HttpContext.Session.GetInt32("user_id");
        //if (!currentUserId.HasValue) return RedirectToAction("New", "Sessions");

        if (!currentUserId.HasValue)
        {
            TempData["ErrorMessage"] = "you must be logged in to write comments!";
            return Redirect($"/recipe/{recipeId}");
        }
        
        _db.Comments.Add(new Comment
        {
            UserId = currentUserId.Value,
            RecipeId = recipeId,
            Content = content.Trim(),
            CreatedOn = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return Redirect($"/recipe/{recipeId}"); // back to details
    }
    
    
    
    // [HttpGet]
    // public async Task<IActionResult> Index(int id)
    // {
    //     var recipe = await _db.Recipes
    //         .Include(r => r.Chef)
    //         .Include(r => r.RecipeIngredients)
    //             .ThenInclude(ri => ri.Ingredient)
    //         .FirstOrDefaultAsync(r => r.Id == id);
    //     if (recipe == null)
    //         return NotFound();
    //     var comments = await _db.Comments
    //         .Where(c => c.RecipeId == id && !string.IsNullOrWhiteSpace(c.Content))
    //         .OrderByDescending(c => c.CreatedOn)
    //         .Select(c => new CommentingViewModel
    //         {
    //             Id = c.Id,
    //             RecipeId = c.RecipeId,
    //             Content = c.Content,
    //             CreatedOn = c.CreatedOn
    //         })
    //         .ToListAsync();
    //     var vm = new IndividualRecipePageViewModel
    //     {
    //         IndividualRecipe = new IndividualRecipeViewModel
    //         {
    //             Id = recipe.Id,
    //             Title = recipe.Title,
    //             ChefName = recipe.Chef.Name,
    //             Description = recipe.Description,
    //         },
    //         CommentsList = comments
        
    //     };
    //     return View("~/Views/RecipeDetails/Index.cshtml", vm);
    // }
}


