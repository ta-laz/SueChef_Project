using Microsoft.AspNetCore.Mvc;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace SueChef.Controllers;

public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly IPasswordHasher<User> _hasher;
    private readonly SueChefDbContext _db;

    public UsersController(ILogger<UsersController> logger, IPasswordHasher<User> hasher, SueChefDbContext db)
    {
        _logger = logger;
        _hasher = hasher;
        _db = db;
    }

    [Route("/signup")]
    [HttpGet]
    public IActionResult New()
    {
        int? id = HttpContext.Session.GetInt32("user_id");
        if (id != null)
        {
            return Redirect("/");
        }
        return View();
    }

    [Route("/users")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SignUpViewModel suvm)
    {
        if (!ModelState.IsValid)
        {
            return View("New", suvm);
        }

        if (await _db.Users.AnyAsync(u => u.Email == suvm.Email))
        {
            ModelState.AddModelError("", "Email already registered.");
            return View("New", suvm);
        }

        if (await _db.Users.AnyAsync(u => u.UserName == suvm.UserName))
        {
            ModelState.AddModelError("", "Username already registered.");
            return View("New", suvm);
        }

        User user = new User
        {
            UserName = suvm.UserName,
            Email = suvm.Email,
            DOB = suvm.DOB,
            DateJoined = DateOnly.FromDateTime(DateTime.Today)
        };


        user.PasswordHash = _hasher.HashPassword(user, suvm.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        HttpContext.Session.SetInt32("user_id", user.Id);
        return new RedirectResult("/");
    }
}