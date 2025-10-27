namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.Test;
using SueChef.TestHelpers;
using SueChef.ViewModels;

public class RecipeCardViewModelTests : PageTest
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
    public async Task JustMinutes_RecipeCardViewModel_TotalTimeJustMinutes()
    {
        RecipeCardViewModel recipe = new() { Id = 1, Title = "test", Description = "great food", DifficultyLevel = 1, IsVegetarian = true, IsDairyFree = true, PrepTime = 10, CookTime = 10 };
        Assert.That(recipe.TotalTimeDisplay, Is.EqualTo("20 mins"));
    }

    [Test]
    public async Task OneHour_RecipeCardViewModel_TotalTimeOneHr()
    {
        RecipeCardViewModel recipe = new() { Id = 1, Title = "test", Description = "great food", DifficultyLevel = 1, IsVegetarian = true, IsDairyFree = true, PrepTime = 30, CookTime = 30 };
        Assert.That(recipe.TotalTimeDisplay, Is.EqualTo("1 hr"));
    }

    [Test]
    public async Task MoreThanOneHour_RecipeCardViewModel_TotalTimeOneHrAndMinutes()
    {
        RecipeCardViewModel recipe = new() { Id = 1, Title = "test", Description = "great food", DifficultyLevel = 1, IsVegetarian = true, IsDairyFree = true, PrepTime = 40, CookTime = 30 };
        Assert.That(recipe.TotalTimeDisplay, Is.EqualTo("1 hr 10 mins"));
    }

    [Test]
    public async Task TwoHours_RecipeCardViewModel_TotalTimeTwoHrs()
    {
        RecipeCardViewModel recipe = new() { Id = 1, Title = "test", Description = "great food", DifficultyLevel = 1, IsVegetarian = true, IsDairyFree = true, PrepTime = 60, CookTime = 60 };
        Assert.That(recipe.TotalTimeDisplay, Is.EqualTo("2 hrs"));
    }
    
    [Test]
    public async Task MoreThanTwoHours_RecipeCardViewModel_TotalTimeTwoHrsAndMins()
    {
        RecipeCardViewModel recipe = new() { Id = 1, Title = "test", Description = "great food", DifficultyLevel = 1, IsVegetarian = true, IsDairyFree = true, PrepTime = 70, CookTime = 60 };
            Assert.That(recipe.TotalTimeDisplay, Is.EqualTo("2 hrs 10 mins"));
    }

}