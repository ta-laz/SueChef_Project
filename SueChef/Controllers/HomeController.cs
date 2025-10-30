using Microsoft.AspNetCore.Mvc;
using SueChef.Services;
using SueChef.Models;
using SueChef.ViewModels;
using System.Diagnostics;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHomePageService _homeService;

    public HomeController(ILogger<HomeController> logger, IHomePageService homeService)
    {
        _logger = logger;
        _homeService = homeService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("user_id");
        var vm = await _homeService.GetHomePageViewModelAsync(userId);
        return View(vm);
    }

    public IActionResult Privacy() => View();
    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        ViewData["Title"] = statusCode switch
        {
            404 => "Page Not Found",
            500 => "Server Error",
            _ => "Error"
        };
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
