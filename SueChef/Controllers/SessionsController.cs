using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SueChef.Models;
using SueChef.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SueChef.Controllers;

public class SessionsController : Controller
{
  private readonly ILogger<SessionsController> _logger;
  private readonly IPasswordHasher<User> _hasher;
  private readonly SueChefDbContext _db;

  public SessionsController(ILogger<SessionsController> logger, IPasswordHasher<User> hasher, SueChefDbContext db)
  {
    _logger = logger;
    _hasher = hasher;
    _db = db;
  }

  [Route("/signin")]
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

  [Route("/signin")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(SignInViewModel sivm)
  {
    if (!ModelState.IsValid)
    {
      return View("New", sivm);
    }

    User? user = await _db.Users.FirstOrDefaultAsync(user => user.UserName == sivm.UserName);
    if (user == null)
    {
      ModelState.AddModelError("", "Incorrect username or password.");
      return View("New", sivm);
    }

    var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, sivm.Password);
    if (verify == PasswordVerificationResult.Failed)
    {
        ModelState.AddModelError("", "Incorrect username or password.");
        return View("New", sivm);
    }
    if (verify == PasswordVerificationResult.SuccessRehashNeeded)
    {
        user.PasswordHash = _hasher.HashPassword(user, sivm.Password);
        await _db.SaveChangesAsync();
    }

    HttpContext.Session.SetInt32("user_id", user.Id);
    return Redirect("/");
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }


  [Route("/Signout")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Signout()
  {
    HttpContext.Session.Clear();

    return RedirectToAction("Index", "Home");
  }
}