namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;            
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.Test;
using SueChef.TestHelpers;


public class SignInOutTests : PageTest
{
    private const string BaseUrl = "http://127.0.0.1:5179";

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await using var context = DbFactory.Create();
        await TestDataSeeder.EnsureDbReadyAsync(context);

    }

    [SetUp]
    public async Task SetupDb()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await using var context = DbFactory.Create();
        await TestDataSeeder.ResetAndSeedAsync(context);
    }
    


    [Test]
    public async Task SignInPage_ShouldLoadCorrectly()
    {
        await Page.GotoAsync($"{BaseUrl}/signin"); //Open the browser and navigate to the sign-in page
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);//wait until the page finishes loading

        var signInHeading = Page.Locator("h2[data-testid='sign-in-text']");
        await signInHeading.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });

        await Expect(signInHeading).ToHaveTextAsync("Sign in");
        await Expect(Page.Locator("[data-testid='username']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='password']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='signin-submit']")).ToBeVisibleAsync();
    }




    [Test]
    public async Task SignInPage_ShowsServerValidation_WhenFieldsEmpty()
    {
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Click Sign In button without filling any fields
        await Page.ClickAsync("[data-testid='signin-submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Expect server-side validation messages on the page
        await Expect(Page.Locator("body")).ToContainTextAsync("Username is required");
        await Expect(Page.Locator("body")).ToContainTextAsync("Password is required");
    }


    [Test]
    public async Task SignIn_ShouldShowError_WhenUsernameMissing()
    {
    // Go to the sign-in page
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Fill only the password field
        await Page.FillAsync("input[name='Password']", "pass");

    // Click the sign-in button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

    // Wait for validation to appear
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Assert that the correct validation message is shown
        await Expect(Page.Locator("body")).ToContainTextAsync("Username is required");
    }


    [Test]
    public async Task SignIn_ShouldShowError_WhenPasswordMissing()
    {
    // Go to the sign-in page
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Fill only the username field
        await Page.FillAsync("input[name='UserName']", "user1");

    // Click the sign-in button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

    // Wait for validation to appear
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Assert that the correct validation message is shown
        await Expect(Page.Locator("body")).ToContainTextAsync("Password is required");
    }




    [Test]
    public async Task SignInPage_ShowsError_WhenInvalidCredentials()
    {
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Fill in invalid credentials
        await Page.FillAsync("[data-testid='username']", "wronguser");
        await Page.FillAsync("[data-testid='password']", "wrongpassword");

    // Click sign in
        await Page.ClickAsync("[data-testid='signin-submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Check that the error message appears on screen
        await Expect(Page.Locator("body")).ToContainTextAsync("Incorrect username or password.");
    }



    [Test]
    public async Task SignUp_ShouldShowError_WhenEmailFormatIsInvalid()
    {
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Fill all fields, but use an invalid email format
        await Page.FillAsync("input[name='UserName']", "invalidEmailUser");
        await Page.FillAsync("input[name='Email']", "notanemail"); // invalid
        await Page.FillAsync("input[name='Password']", "StrongPass1!");
        await Page.FillAsync("input[name='ConfirmPassword']", "StrongPass1!");
        await Page.FillAsync("input[name='DOB']", "1990-05-05");

    // Submit the form
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Capture body text to check for the validation message
        var bodyText = await Page.InnerTextAsync("body");

        Assert.That(
            bodyText.Contains("Enter a valid email address"),
            $"Expected server-side validation message 'Enter a valid email address', but got:\n\n{bodyText}"
        );
    }




    [Test]
    public async Task SignInPage_CreateAccountLink_NavigatesToSignUp()
    {
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Click the signup link
        await Page.ClickAsync("#signup");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Navigate to Sign Up
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/signup");
    }


    [Test]
    public async Task SignInPage_ShouldShowGoogleLoginButton()
    {
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Find the Google button
        var googleButton = Page.Locator("a[href='/login/google']");

        await Expect(googleButton).ToBeVisibleAsync();
        await Expect(googleButton).ToContainTextAsync("Continue with Google");
    }



    [Test]
    public async Task SignInPage_SignInButton_ShouldBeVisible_and_has_sinInText()
    {
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var signInBtn = Page.Locator("[data-testid='signin-submit']");
        await Expect(signInBtn).ToBeVisibleAsync();
        await Expect(signInBtn).ToHaveTextAsync("Sign In");
    }


    [Test]
    public async Task SignInPage_ShouldDisplaySueChefLogo_AndNavigateHome()
    {
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var logoHeading = Page.Locator("h1", new() { HasTextString = "SueChef" });
        await Expect(logoHeading).ToBeVisibleAsync();

    // Verify it links back home
        var logoLink = Page.Locator("a[href='/'] h1");
        await Expect(logoLink).ToBeVisibleAsync();

    // Click it to verify navigation
        await logoLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
    }




    [Test]
        public async Task SignUpPage_ShouldLoadCorrectly()
        {
            await Page.GotoAsync($"{BaseUrl}/signup");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Expect(Page.Locator("body")).ToContainTextAsync("Sign Up");
            await Expect(Page.Locator("input[name='UserName']")).ToBeVisibleAsync();
            await Expect(Page.Locator("input[name='Email']")).ToBeVisibleAsync();
            await Expect(Page.Locator("input[name='DOB']")).ToBeVisibleAsync();
            await Expect(Page.Locator("input[name='Password']")).ToBeVisibleAsync();
            await Expect(Page.Locator("input[name='ConfirmPassword']")).ToBeVisibleAsync();
            await Expect(Page.Locator("button[type='submit']")).ToBeVisibleAsync();
        }

    [Test]
        public async Task SignUpPage_ShowsValidationErrors_WhenFieldsEmpty()
        {
            await Page.GotoAsync($"{BaseUrl}/signup");
            await Page.ClickAsync("button[type='submit']");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Expect(Page.Locator("body")).ToContainTextAsync("Username is required");
            await Expect(Page.Locator("body")).ToContainTextAsync("Password is required");
        }


        [Test]
        public async Task SignUpPage_ShowsError_WhenPasswordsDoNotMatch()
        {
            await Page.GotoAsync($"{BaseUrl}/signup");

            await Page.FillAsync("input[name='UserName']", "userMismatch");
            await Page.FillAsync("input[name='Email']", "userMismatch@example.com");
            await Page.FillAsync("input[name='DOB']", "2000-01-01");
            await Page.FillAsync("input[name='Password']", "Password123!");
            await Page.FillAsync("input[name='ConfirmPassword']", "Password999!");
            await Page.ClickAsync("button[type='submit']");

            await Expect(Page.Locator("body")).ToContainTextAsync("Passwords do not match");
        }



    [Test]
    public async Task SignUpPage_SuccessfulRegistration_RedirectsHome()
    {
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.FillAsync("input[name='UserName']", "newuser123");
        await Page.FillAsync("input[name='Email']", "newuser123@example.com");
        await Page.FillAsync("input[name='DOB']", "1995-05-05");
        await Page.FillAsync("input[name='Password']", "TestPassword123!");
        await Page.FillAsync("input[name='ConfirmPassword']", "TestPassword123!");
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/"); 
    }


    [Test]
    public async Task SignUpPage_ShouldShowError_WhenEmailAlreadyExists()
    {
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Use an existing seeded user
        await Page.FillAsync("input[name='UserName']", "duplicateUser");
        await Page.FillAsync("input[name='Email']", "user1@example.com");
        await Page.FillAsync("input[name='DOB']", "1990-01-01");
        await Page.FillAsync("input[name='Password']", "Password123!");
        await Page.FillAsync("input[name='ConfirmPassword']", "Password123!");
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //  The page remains on the POST URL (/users) when model validation fails
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users");

    // Verify the validation message is displayed
        await Expect(Page.Locator("body")).ToContainTextAsync("Email already registered");
    }


    [Test]
    public async Task SignUpPage_ShouldShowGoogleLoginButton()
    {
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var googleBtn = Page.Locator("a[href='/login/google']");
        await Expect(googleBtn).ToBeVisibleAsync();
        await Expect(googleBtn).ToContainTextAsync("Continue with Google");
    }

    [Test]
    public async Task SignUpPage_ShouldShowError_WhenRequiredFieldsMissing()
    {
    // Ensure no leftover session redirects to home
        await Context.ClearCookiesAsync();

    // Go to signup page
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Wait for form to appear
        await Page.WaitForSelectorAsync("form", new() { Timeout = 10000 });
        await Page.WaitForSelectorAsync("button, input[type='submit']", new() { Timeout = 10000 });

    // Click Sign Up without filling anything
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Check validation messages (actual rendered text)
        await Expect(Page.Locator("body")).ToContainTextAsync("Username is required");
        await Expect(Page.Locator("body")).ToContainTextAsync("Email is required");
        await Expect(Page.Locator("body")).ToContainTextAsync("Password is required");
        await Expect(Page.Locator("body")).ToContainTextAsync("Confirmation is required");
        var bodyText = await Page.InnerTextAsync("body");
        Assert.That(
            bodyText.Contains("The value '' is invalid.") ||
            bodyText.Contains("Date of Birth is required"),
            "Expected a Date of Birth validation error message but none was found."
        );
    }



    [Test]
    public async Task SignUpPage_ShouldShowError_WhenUserTooYoung()
    {
        await Context.ClearCookiesAsync(); // ensure no session redirects
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.WaitForSelectorAsync("input[name='UserName']", new() { Timeout = 10000 });

        var underageDate = DateTime.Now.AddYears(-10).ToString("yyyy-MM-dd");
        var uniqueEmail = $"young{DateTime.Now.Ticks}@example.com";

        await Page.FillAsync("input[name='UserName']", "youngUser");
        await Page.FillAsync("input[name='Email']", uniqueEmail);
        await Page.FillAsync("input[name='DOB']", underageDate);
        await Page.FillAsync("input[name='Password']", "Password123!");
        await Page.FillAsync("input[name='ConfirmPassword']", "Password123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/users");
        await Expect(Page.Locator("body")).ToContainTextAsync("You must be at least 12 years old");
    }




    [Test]
    public async Task SignUp_ShouldShowError_WhenPasswordTooShortOrWeak()
    {
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Fill form with a weak password (too short, no uppercase/special char)
        await Page.FillAsync("input[name='UserName']", "weakpassworduser");
        await Page.FillAsync("input[name='Email']", "weakpass@example.com");
        await Page.FillAsync("input[name='Password']", "pass"); // invalid
        await Page.FillAsync("input[name='ConfirmPassword']", "pass");
        await Page.FillAsync("input[name='DOB']", "1995-03-15");

    // Try to submit the form
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Capture body text to inspect server validation messages
        var bodyText = await Page.InnerTextAsync("body");

        Assert.That(
            bodyText.Contains("Password must be between 8 and 50 characters and include an uppercase letter and a special character."),
            $"Expected validation message about weak password, but got:\n\n{bodyText}"
        );
    }



    [Test]
    public async Task SignUpPage_ShouldDisplaySueChefLogo_AndNavigateHome()
    {
    // Go to the sign-up page
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //  Locate the "SueChef" logo text inside the <h1>
        var logoHeading = Page.Locator("h1", new() { HasTextString = "SueChef" });
        await Expect(logoHeading).ToBeVisibleAsync();

    // Ensure the logo links back to the homepage
        var logoLink = Page.Locator("a[href='/'] h1");
        await Expect(logoLink).ToBeVisibleAsync();

    // Click to confirm navigation
        await logoLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
    }


    [Test]
    public async Task HomePage_AccountIcon_ShouldOpenMenuWithSignInAndRegisterLinks()
    {
    // Navigate to the homepage
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Ensure the account icon (chef hat) is visible
        var accountIcon = Page.Locator("#accountButton img[alt='Account icon']");
        await Expect(accountIcon).ToBeVisibleAsync();

    // Click the icon to open the account menu
        await Page.ClickAsync("#accountButton");

    // Check that the side account menu appears
        var accountMenu = Page.Locator("#sideMenuAccount");
        await Expect(accountMenu).ToBeVisibleAsync();

    // Verify both "Sign In" and "Register" links are visible
        var signInLink = Page.Locator("a[href='/signin'], a[asp-action='New'][asp-controller='Sessions']");
        var signUpLink = Page.Locator("a[href='/signup'], a[asp-action='New'][asp-controller='Users']");

        await Expect(signInLink).ToBeVisibleAsync();
        await Expect(signUpLink).ToBeVisibleAsync();

    // ✅ Optional navigation checks (can comment out if routes differ)
        await signInLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/signin");

    // Return to home to test Register link
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.ClickAsync("#accountButton");
        await signUpLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/signup");
    }

}














    

















