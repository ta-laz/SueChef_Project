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

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            var difficultyLevelText = await recipeCards.Nth(i).Locator("[data-testid='difficulty-level']").TextContentAsync();

            Assert.That("Easy", Is.EqualTo(difficultyLevelText.Trim()));
            Assert.That("Medium", Is.Not.EqualTo(difficultyLevelText.Trim()));
            Assert.That("Hard", Is.Not.EqualTo(difficultyLevelText.Trim()));
        }
    }

    [Test]
    public async Task MediumCategoryParameter_CategoriesPage_ShowsOnlyMediumRecipes()
    {
        await Page.GotoAsync("/Categories?category=medium");
        await Expect(Page.GetByTestId("Title-Medium Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            var difficultyLevelText = await recipeCards.Nth(i).Locator("[data-testid='difficulty-level']").TextContentAsync();

            Assert.That("Medium", Is.EqualTo(difficultyLevelText.Trim()));
            Assert.That("Easy", Is.Not.EqualTo(difficultyLevelText.Trim()));
            Assert.That("Hard", Is.Not.EqualTo(difficultyLevelText.Trim()));
        }
    }

    [Test]
    public async Task HardCategoryParameter_CategoriesPage_ShowsOnlyHardRecipes()
    {
        await Page.GotoAsync("/Categories?category=hard");
        await Expect(Page.GetByTestId("Title-Hard Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            var difficultyLevelText = await recipeCards.Nth(i).Locator("[data-testid='difficulty-level']").TextContentAsync();

            Assert.That("Hard", Is.EqualTo(difficultyLevelText.Trim()));
            Assert.That("Easy", Is.Not.EqualTo(difficultyLevelText.Trim()));
            Assert.That("Medium", Is.Not.EqualTo(difficultyLevelText.Trim()));
        }
    }
}