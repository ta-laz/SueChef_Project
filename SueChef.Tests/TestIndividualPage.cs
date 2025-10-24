namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.Test;
using SueChef.TestHelpers;

public class PlaywrightRecipeTests : PageTest
{
    private const string BaseUrl = "http://127.0.0.1:5179";

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await using var context = DbFactory.Create();
        await TestDataSeeder.EnsureDbReadyAsync(context);
    }

    [Test]
    public async Task IndividualPage_ShowsChickenTikkaMasalaTitle()
    {
        var recipeId = 2;

        // Navigate to the recipe page
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Verify that the correct title is shown
        await Expect(Page.Locator("body"))
            .ToContainTextAsync("Authentic Chicken Tikka Masala Curry");
    }


    [Test]
    public async Task IndividualPage_ShowsIngredientsTabTitle()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Look for the Ingredients section heading
        await Expect(Page.Locator("body"))
            .ToContainTextAsync("Ingredients");
    }

    [Test]
    public async Task IndividualPage_ShowsChickenBreastIngredientAndMeasurement()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Make sure the ingredient "Chicken Breast" appears
        await Expect(Page.Locator("body"))
            .ToContainTextAsync("Chicken Breast");

        // And confirm its measurement appears as well 
        await Expect(Page.Locator("body"))
            .ToContainTextAsync("600g");
    }


    [Test]
    public async Task IndividualPage_ShowsRateTheRecipeSection()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        await Expect(Page.Locator("body"))
        .ToContainTextAsync("Rate this recipe");
    }


    [Test]
    public async Task IndividualPage_ShowsNutritionInfo_WhenNutritionTabClicked()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Click the Nutrition tab 
        await Page.GetByText("Nutrition").ClickAsync();

        // Expect the nutrition labels to be visible
        await Expect(Page.Locator("body")).ToContainTextAsync("Calories per Serving");
        await Expect(Page.Locator("body")).ToContainTextAsync("Protein per Serving");
        await Expect(Page.Locator("body")).ToContainTextAsync("Carbs per Serving");
        await Expect(Page.Locator("body")).ToContainTextAsync("Fats per Serving");
        await Expect(Page.Locator("body")).ToContainTextAsync("904"); // Calories
        await Expect(Page.Locator("body")).ToContainTextAsync("62.5");  // Protein
        await Expect(Page.Locator("body")).ToContainTextAsync("82.9");  // Carbs
        await Expect(Page.Locator("body")).ToContainTextAsync("34.9");  // Fats
    }


    [Test]
    public async Task IndividualPage_ShowsRatingForm_WhenRateTheRecipeClicked()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Scroll to the bottom to make sure the "Rate the Recipe" button is visible
        await Page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");

        // Click the "Rate the Recipe" button or link
        await Page.GetByText("Rate this recipe").ClickAsync();

        // Expect the rating form or modal to appear
        await Expect(Page.Locator("body")).ToContainTextAsync("Rating");

        // check if rating input is visible
        await Expect(Page.Locator("input[type='number'], .star-rating, .rating-stars"))
        .ToBeVisibleAsync();


    }
}




