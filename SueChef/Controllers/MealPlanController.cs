using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;


namespace SueChef.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class MealPlanController : Controller
{
    private readonly ILogger<MealPlanController> _logger;
    private readonly SueChefDbContext _db;

    public MealPlanController(ILogger<MealPlanController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }


    [Route("/MealPlans")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View();
    }


    [Route("/MealPlans")]
    [HttpPost]
    public async Task<IActionResult> Create(MealPlan mealPlan)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        mealPlan.UserId = currentUserId;
        _db.MealPlans.Add(mealPlan);
        await _db.SaveChangesAsync();
        return View("Index");
    }

}