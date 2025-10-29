using Microsoft.AspNetCore.Mvc;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;


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

    [Route("/signin-google")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (email == null)
            return Redirect("/signup"); // fallback

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                Email = email,
                UserName = name ?? email.Split('@')[0],
                DateJoined = DateOnly.FromDateTime(DateTime.Today)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        HttpContext.Session.SetInt32("user_id", user.Id);
        return Redirect("/");
    }

    [HttpGet("/login/google")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }


    // These are the parts related to the Account Settings Page
    [ServiceFilter(typeof(AuthenticationFilter))]
    [HttpGet("/users/{id}/settings")]
    public IActionResult AccountSettings(int id)
    {
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        if (sessionUserId == null || sessionUserId != id)
            return Redirect($"/users/{sessionUserId}/settings");

        var user = _db.Users.Find(id);
        if (user == null) return NotFound();

        var vm = new AccountSettingsViewModel
        {
            Id = user.Id,
            CurrentUserName = user.UserName,
            CurrentEmail = user.Email,
            DateJoined = user.DateJoined,
            SuccessMessage = TempData["SuccessMessage"] as string,
            DeleteError = TempData["DeleteError"] as string
        };

        return View("AccountSettings", vm);
    }

    [HttpPost("/users/update-username")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateUsername([Bind(Prefix = "ChangeUsername")] ChangeUsernameViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["DeleteError"] = "Please fill in all required fields.";
            return Redirect($"/users/{model.Id}/settings");
        }

        // check current session user
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        if (sessionUserId == null || sessionUserId != model.Id)
            return Redirect($"/users/{sessionUserId}/settings");

        var user = _db.Users.Find(model.Id);
        if (user == null) return NotFound();

        // verify password
        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash!, model.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            TempData["DeleteError"] = "Incorrect password.";
            return Redirect($"/users/{user.Id}/settings");
        }

        // check that username isn't already in use
        bool nameTaken = _db.Users.Any(u => u.UserName == model.NewUserName && u.Id != user.Id);
        if (nameTaken)
        {
            TempData["DeleteError"] = "That username is already taken.";
            return Redirect($"/users/{user.Id}/settings");
        }

        // update and save
        user.UserName = model.NewUserName;
        _db.SaveChanges();

        TempData["SuccessMessage"] = "Username updated successfully.";
        return Redirect($"/users/{user.Id}/settings");
    }
    [HttpPost("/users/update-email")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateEmail([Bind(Prefix = "ChangeEmail")] ChangeEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["DeleteError"] = "Please enter a valid email and password.";
            return Redirect($"/users/{model.Id}/settings");
        }

        // is person logged in the right person (feels extra but good practice)
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        if (sessionUserId == null || sessionUserId != model.Id)
            return Redirect($"/users/{sessionUserId}/settings");

        var user = _db.Users.Find(model.Id);
        if (user == null) return NotFound();

        // check passwork
        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash!, model.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            TempData["DeleteError"] = "Incorrect password.";
            return Redirect($"/users/{user.Id}/settings");
        }

        // check that the email isn't being used
        bool emailTaken = _db.Users.Any(u => u.Email == model.NewEmail && u.Id != user.Id);
        if (emailTaken)
        {
            TempData["DeleteError"] = "That email is already registered.";
            return Redirect($"/users/{user.Id}/settings");
        }

        // save changes
        user.Email = model.NewEmail;
        _db.SaveChanges();

        TempData["SuccessMessage"] = "Email updated successfully.";
        return Redirect($"/users/{user.Id}/settings");
    }

    [HttpPost("/users/update-password")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdatePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model)
    {
        // check model validation
        if (!ModelState.IsValid)
        {
            TempData["DeleteError"] = "Please fix the errors in the password form.";
            return Redirect($"/users/{model.Id}/settings");
        }

        // confirm they’re logged in as the right user
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        if (sessionUserId == null || sessionUserId != model.Id)
            return Redirect($"/users/{sessionUserId}/settings");

        var user = _db.Users.Find(model.Id);
        if (user == null) return NotFound();

        // verify the current password
        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash!, model.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            TempData["DeleteError"] = "Current password is incorrect.";
            return Redirect($"/users/{user.Id}/settings");
        }

        // hash and set the new password
        user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
        _db.SaveChanges();

        // success message + redirect
        TempData["SuccessMessage"] = "Password updated successfully.";
        return Redirect($"/users/{user.Id}/settings");
    }

    [HttpPost("/users/delete-account")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteAccount([Bind(Prefix = "DeleteAccount")] DeleteAccountViewModel model)
    {
        // validate basic input
        if (!ModelState.IsValid)
        {
            TempData["DeleteError"] = "Please enter your password to confirm deletion.";
            return Redirect($"/users/{model.Id}/settings");
        }

        // make sure the session user matches
        int? sessionUserId = HttpContext.Session.GetInt32("user_id");
        if (sessionUserId == null || sessionUserId != model.Id)
            return Redirect($"/users/{sessionUserId}/settings");

        var user = _db.Users.Find(model.Id);
        if (user == null) return NotFound();

        // verify password before deletion
        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash!, model.ConfirmDeletePassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            TempData["DeleteError"] = "Incorrect password. Account not deleted.";
            return Redirect($"/users/{user.Id}/settings");
        }

        // delete the user
        _db.Users.Remove(user);
        _db.SaveChanges();

        // clear session and redirect
        HttpContext.Session.Clear();

        TempData["SuccessMessage"] = "Your account has been deleted successfully.";
        return Redirect("/");
    }


}

