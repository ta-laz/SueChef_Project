using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;


namespace SueChef.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class ShoppingListController : Controller
{
    private readonly ILogger<ShoppingListController> _logger;
    private readonly SueChefDbContext _db;

    public ShoppingListController(ILogger<ShoppingListController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Generate(List<RecipeCardViewModel>? RecipesList)
    {
        Dictionary<string,decimal> ShoppingList = new Dictionary<string, decimal>();

        foreach (var recipe in RecipesList)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!ShoppingList.ContainsKey(ingredient.Name))
                {
                    ShoppingList[ingredient.Name] = ingredient.Quantity;
                }
                ShoppingList[ingredient.Name] += ingredient.Quantity;
            }
        }

        return View();
    }
}