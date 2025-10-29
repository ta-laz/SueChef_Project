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
    public async Task EasyCategoryPage_CategoriesPage_ShowsOnlyEasyRecipes()
    {
        await Page.GotoAsync("/Categories?category=easy");
        await Expect(Page.GetByTestId("Title-Easy Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            var difficultyLevelText = await recipeCards.Nth(i).Locator("[data-testid='difficulty-level']").TextContentAsync();

            Assert.That(difficultyLevelText.Trim(), Is.EqualTo("Easy"));
            Assert.That(difficultyLevelText.Trim(), Is.Not.EqualTo("Medium"));
            Assert.That(difficultyLevelText.Trim(), Is.Not.EqualTo("Hard"));
        }
    }

    [Test]
    public async Task MediumCategoryPage_CategoriesPage_ShowsOnlyMediumRecipes()
    {
        await Page.GotoAsync("/Categories?category=medium");
        await Expect(Page.GetByTestId("Title-Medium Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            var difficultyLevelText = await recipeCards.Nth(i).Locator("[data-testid='difficulty-level']").TextContentAsync();

            Assert.That(difficultyLevelText.Trim(), Is.EqualTo("Medium"));
            Assert.That(difficultyLevelText.Trim(), Is.Not.EqualTo("Easy"));
            Assert.That(difficultyLevelText.Trim(), Is.Not.EqualTo("Hard"));
        }
    }

    [Test]
    public async Task HardCategoryPage_CategoriesPage_ShowsOnlyHardRecipes()
    {
        await Page.GotoAsync("/Categories?category=hard");
        await Expect(Page.GetByTestId("Title-Hard Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            var difficultyLevelText = await recipeCards.Nth(i).Locator("[data-testid='difficulty-level']").TextContentAsync();

            Assert.That(difficultyLevelText.Trim(), Is.EqualTo("Hard"));
            Assert.That(difficultyLevelText.Trim(), Is.Not.EqualTo("Easy"));
            Assert.That(difficultyLevelText.Trim(), Is.Not.EqualTo("Medium"));
        }
    }

    [Test]
    public async Task QuickCategoryPage_CategoriesPage_ShowsOnlyQuickRecipes()
    {
        await Page.GotoAsync("/Categories?category=quick");
        await Expect(Page.GetByTestId("Title-Quick Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            var recipeTimeText = await recipeCards.Nth(i).Locator("[data-testid='recipe-time']").TextContentAsync();

            Assert.That(recipeTimeText.Trim(), Does.Not.Contain("hr"));
        }
    }

    [Test]
    public async Task HighlyRatedCategoryPage_CategoriesPage_ShowsHighestRatedRecipeFirst()
    {
        await Page.GotoAsync("/Categories?category=highlyrated");
        await Expect(Page.GetByTestId("Title-Top 10 Recipes")).ToBeVisibleAsync();

        // expect that the first recipe title on the page is the highest rated recipe
        // this may change after seeds change
        await Expect(Page.GetByTestId("recipe-title-div").First).ToContainTextAsync("Caprese Tomato Mozzarella Salad");
    }

    [Test]
    public async Task MostPopularCategoryPage_CategoriesPage_ShowsMostPopularRecipeFirst()
    {
        await Page.GotoAsync("/Categories?category=mostpopular");
        await Expect(Page.GetByTestId("Title-Most Popular Recipes")).ToBeVisibleAsync();

        // expect that the first recipe title on the page is the most rated recipe
        // this may change after seeds change
        await Expect(Page.GetByTestId("recipe-title-div").First).ToContainTextAsync("Korean Bibimbap Rice Bowl");
    }

    [Test]
    public async Task RateRecipe_CategoriesPage_UpdatesMostPopularRecipe()
    {
        await Page.GotoAsync("/Categories?category=mostpopular");
        await Expect(Page.GetByTestId("Title-Most Popular Recipes")).ToBeVisibleAsync();

        // expect that the first recipe title on the page is the most rated recipe
        // this may change after seeds change
        await Expect(Page.GetByTestId("recipe-title-div").First).ToContainTextAsync("Korean Bibimbap Rice Bowl");

        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.GetByTestId("username").FillAsync("TestingUser");
        await Page.GetByTestId("dob").FillAsync("1995-08-10");
        await Page.GetByTestId("email").FillAsync("test@testmail.com");
        await Page.GetByTestId("password").FillAsync("Password123!");
        await Page.GetByTestId("confirmpassword").FillAsync("Password123!");
        await Page.GetByTestId("signup-submit").ClickAsync();

        await Page.GotoAsync("/Recipe/29");
        await Page.GetByTestId("star-rating-1").ClickAsync();
        await Page.GetByTestId("rating-submit-button").ClickAsync();
        await Expect(Page.GetByTestId("thanks-for-rating")).ToBeVisibleAsync();

        await Page.GotoAsync("/Categories?category=mostpopular");
        await Expect(Page.GetByTestId("Title-Most Popular Recipes")).ToBeVisibleAsync();

        // expect that the first recipe title on the page is the recipe that's just been rated
        // this may change after seeds change
        await Expect(Page.GetByTestId("recipe-title-div").First).ToContainTextAsync("Oven-Baked Tandoori Salmon");
    }

    [Test]
    public async Task DairyFreeCategoryPage_CategoriesPage_ShowsOnlyDairyFreeRecipes()
    {
        await Page.GotoAsync("/Categories?category=dairyfree");
        await Expect(Page.GetByTestId("Title-Dairy-Free Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            await recipeCards.Nth(i).Locator("[data-testid='dairy-free-tag']").IsVisibleAsync();
        }
    }

    [Test]
    public async Task VegetarianCategoryPage_CategoriesPage_ShowsOnlyVegetarianRecipes()
    {
        await Page.GotoAsync("/Categories?category=vegetarian");
        await Expect(Page.GetByTestId("Title-Vegetarian Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        for (int i = 0; i < recipeCount; i++)
        {
            await recipeCards.Nth(i).Locator("[data-testid='vegetarian-tag']").IsVisibleAsync();
        }
    }

    [Test]
    public async Task AllRecipesCategoryPage_CategoriesPage_ShowsAllRecipes()
    {
        await Page.GotoAsync("/Categories?category=allrecipes");
        await Expect(Page.GetByTestId("Title-All Recipes")).ToBeVisibleAsync();

        var recipeCards = Page.Locator(".recipe-card");

        var recipeCount = await recipeCards.CountAsync();
        Assert.That(recipeCount, Is.EqualTo(58));
    }

    // , vegetarian, all recipes

}