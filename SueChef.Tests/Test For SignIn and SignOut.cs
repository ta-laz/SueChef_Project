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
    public async Task SignInPage_AllowsLogin_WithValidCredentials()
    {
        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Fill in valid credentials from your seeded test user
        await Page.FillAsync("[data-testid='username']", "testuser");
        await Page.FillAsync("[data-testid='password']", "Password123!");

        await Page.ClickAsync("[data-testid='signin-submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // ✅ Expect to be redirected to homepage
        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");

    // ✅ Optionally confirm nav or welcome text appears
        await Expect(Page.Locator("body")).ToContainTextAsync("Welcome back, testuser");
    }












}






