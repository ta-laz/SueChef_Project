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
                Page.GetByTestId("category-easy").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=easy")
            );
        await Expect(Page.GetByTestId("Title-Easy Recipes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MediumCategory_HomePage_RedirectsMediumCategoryPage()
    {
        await Task.WhenAll(
                Page.GetByTestId("category-medium").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=medium")
            );
        await Expect(Page.GetByTestId("Title-Medium Recipes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HardCategory_HomePage_RedirectsHardCategoryPage()
    {
        await Task.WhenAll(
                Page.GetByTestId("category-hard").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=hard")
            );
        await Expect(Page.GetByTestId("Title-Hard Recipes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task QuickCategory_HomePage_RedirectsQuickCategoryPage()
    {
        await Task.WhenAll(
                Page.GetByTestId("category-quick").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=quick")
            );
        await Expect(Page.GetByTestId("Title-Quick Recipes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HighlyRatedCategory_HomePage_RedirectsHighlyRatedCategoryPage()
    {
        await Task.WhenAll(
                Page.GetByTestId("category-highlyrated").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=highlyrated")
            );
        await Expect(Page.GetByTestId("Title-Top 10 Recipes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MostPopularCategory_HomePage_RedirectsMostPopularCategoryPage()
    {
        await Task.WhenAll(
                Page.GetByTestId("category-mostpopular").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=mostpopular")
            );
        await Expect(Page.GetByTestId("Title-Most Popular Recipes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task DairyFreeCategory_HomePage_RedirectsDairyFreeCategoryPage()
    {
        await Task.WhenAll(
                Page.GetByTestId("category-dairyfree").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=dairyfree")
            );
        await Expect(Page.GetByTestId("Title-Dairy-Free Recipes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task VegetarianCategory_HomePage_RedirectsVegetarianCategoryPage()
    {
        await Page.GetByTestId("category-carousel-right-button").ClickAsync();

        await Page.WaitForSelectorAsync(".carousel-track");

        await Page.GetByTestId("category-vegetarian").First.ScrollIntoViewIfNeededAsync();
        await Task.WhenAll(
                Page.GetByTestId("category-vegetarian").First.ClickAsync(),
                Page.WaitForURLAsync($"{BaseUrl}/Categories?category=vegetarian")
            );
        await Expect(Page.GetByTestId("Title-Vegetarian Recipes")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CategoryCarouselRightButton_HomePage_IsVisible()
    {
        await Expect(Page.GetByTestId("category-carousel-right-button")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CategoryCarouselLeftButton_HomePage_IsVisible()
    {
        await Expect(Page.GetByTestId("category-carousel-left-button")).ToBeVisibleAsync();
    }

    [Test]
    public async Task RecipeCarouselLeftButton_HomePage_IsVisible()
    {
        await Expect(Page.GetByTestId("recipe-carousel-left-button").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task RecipeCarouselRightButton_HomePage_IsVisible()
    {
        await Expect(Page.GetByTestId("recipe-carousel-right-button").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task EasyRecipesUnderCarousel_HomePage_RedirectsToEasyCategoriesPage()
    {
        await Page.GetByTestId("easy-recipes-under-carousel-button").ClickAsync();
        await Expect(Page.GetByTestId("Title-Easy Recipes")).ToBeVisibleAsync();
    }
}