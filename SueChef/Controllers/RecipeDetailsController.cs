using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;


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

        if (recipe == null)
        {
            return NotFound();
        }

        return View(recipe);
    }
}