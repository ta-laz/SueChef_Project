namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.Test;
using SueChef.TestHelpers;
using SueChef.ViewModels;

public class HomePage : PageTest
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
        await Page.GotoAsync("/");
        await using var context = DbFactory.Create();
        await TestDataSeeder.ResetAndSeedAsync(context);
    }
    public override BrowserNewContextOptions ContextOptions()
      => new BrowserNewContextOptions
      {
          BaseURL = BaseUrl
      };

    [Test]
    public async Task Title_HomePage_IsHomePage()
    {
        await Expect(Page).ToHaveTitleAsync(new Regex("Home Page"));
    }

    [Test]
    public async Task TopFeaturedRecipe_HomePage_RedirectsToRecipe41()
    {
        await Task.WhenAll(
                Page.GetByTestId("Featured Recipe 41").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Recipe/41")
            );
        await Expect(Page.GetByTestId("recipe-title Fluffy Buttered Breakfast Pancakes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MiddleFeaturedRecipe_HomePage_RedirectsToRecipe47()
    {
        await Task.WhenAll(
                Page.GetByTestId("Featured Recipe 47").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Recipe/47")
            );
        await Expect(Page.GetByTestId("recipe-title Spinach Ricotta Cannelloni Bake")).ToBeVisibleAsync();
    }

    [Test]
    public async Task BottomFeaturedRecipe_HomePage_RedirectsToRecipe2()
    {
        await Task.WhenAll(
                Page.GetByTestId("Featured Recipe 2").ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Recipe/2")
            );
        await Expect(Page.GetByTestId("recipe-title Authentic Chicken Tikka Masala Curry")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task EasyCategory_HomePage_RedirectsEasyCategoryPage()
    {
        await Task.WhenAll(
                Page.GetByText("Easy Recipes").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=easy")
            );
        await Expect(Page.GetByText("Easy Recipes")).ToBeVisibleAsync();
    }
}