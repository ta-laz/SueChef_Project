namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.Test;
using SueChef.TestHelpers;
using SueChef.ViewModels;

public class CategoryPage : PageTest
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
    public async Task Title_CategoriesPage_IsCategoriesPage()
    {
        await Page.GotoAsync("/Categories?category=easy");
        await Expect(Page).ToHaveTitleAsync(new Regex("Categories Page"));
    }

    [Test]
    public async Task EasyCategoryParameter_CategoriesPage_ShowsOnlyEasyRecipes()
    {
        await Page.GotoAsync("/Categories?category=easy");
        await Expect(Page.GetByTestId("Title-Easy Recipes")).ToBeVisibleAsync();
    }
}