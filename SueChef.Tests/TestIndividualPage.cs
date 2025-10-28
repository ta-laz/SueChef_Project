namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;            
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

    [SetUp]
    public async Task SetupDb()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await using var context = DbFactory.Create();
        await TestDataSeeder.ResetAndSeedAsync(context);
    }

    [Test]
    public async Task IndividualPage_ShowsChickenTikkaMasalaTitle()
    {
        var recipeId = 2;

        // Navigate to the recipe page
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Verify that the correct title is shown
        await Expect(Page.GetByTestId("recipe-title"))
            .ToContainTextAsync("Authentic Chicken Tikka Masala Curry");
    }


    [Test]
    public async Task IndividualPage_ShowsIngredientsTabTitle()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Look for the Ingredients section heading
        await Expect(Page.GetByTestId("recipe-tab-toggle"))
            .ToContainTextAsync("Ingredients");
    }

    [Test]
    public async Task IndividualPage_ShowsChickenBreastIngredientAndMeasurement()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Make sure the ingredient "Chicken Breast" appears and 600g 
        await Expect(Page.GetByTestId("recipe-ingredients-Chicken Breast")).ToContainTextAsync("600g Chicken Breast");
    }


    [Test]
    public async Task IndividualPage_ShowsRateTheRecipeSection()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        await Expect(Page.GetByTestId("Rate-the-recipe"))
        .ToContainTextAsync("Rate this recipe");
    }


    [Test]
    public async Task IndividualPage_ShowsNutritionInfo_WhenNutritionTabClicked()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        // Click the Nutrition tab 
        await Page.GetByTestId("toggle-nutrition").ClickAsync();

        // Expect the nutrition labels to be visible
        await Expect(Page.GetByTestId("calories")).ToContainTextAsync("Calories 904 Kcal");
        await Expect(Page.GetByTestId("protein")).ToContainTextAsync("Protein 62.5g");
        await Expect(Page.GetByTestId("carbs")).ToContainTextAsync("Carbs 82.9g");
        await Expect(Page.GetByTestId("fats")).ToContainTextAsync("Fats 34.9g");
    }


    [Test]
    public async Task IndividualPage_RedirectsToLoginWhenRating()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        await Page.GetByTestId("star-rating-1").ClickAsync();

        await Task.WhenAll(
        Page.GetByTestId("rating-submit-button").ClickAsync(),
        Page.WaitForURLAsync($"{BaseUrl}/signin")
        );

        await Expect(Page.GetByTestId("sign-in-text")).ToBeVisibleAsync();

    }
    [Test]
    public async Task IndividualPage_LoggedInUserCanRateRecipe()
    {
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.GetByTestId("username").FillAsync("TestingUser");
        await Page.GetByTestId("dob").FillAsync("1995-08-10");
        await Page.GetByTestId("email").FillAsync("test@testmail.com");
        await Page.GetByTestId("password").FillAsync("Password123!");
        await Page.GetByTestId("confirmpassword").FillAsync("Password123!");
        await Page.GetByTestId("submit-signup").ClickAsync();
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        await Page.GetByTestId("star-rating-1").ClickAsync();
        await Page.GetByTestId("rating-submit-button").ClickAsync();
        await Expect(Page.GetByTestId("thanks-for-rating")).ToBeVisibleAsync();
    }
    [Test]
    public async Task IndividualPage_MethodFormatsFromDBCorrectly()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        await Expect(Page.GetByTestId("method-steps")).ToContainTextAsync("Step 1: Marinate chicken Step 2: Grill Step 3: Simmer sauce with tomato, cream and spices Step 4: Combine and simmer.");
    }
    [Test]
    public async Task IndividualPage_ServingsButtonChangesIngredientAmounts()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        // Make sure the ingredient "Chicken Breast" appears and 600g 
        await Expect(Page.GetByTestId("recipe-ingredients-Chicken Breast")).ToContainTextAsync("600g Chicken Breast");
        await Page.GetByTestId("serving-input").FillAsync("1");
        await Expect(Page.GetByTestId("recipe-ingredients-Chicken Breast")).ToContainTextAsync("150g Chicken Breast");

    }

    [Test]
    public async Task IndividualPage_ShowsCommentFormAndButton()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        await Expect(Page.GetByText("Comment on this Recipe")).ToBeVisibleAsync();
        await Expect(Page.Locator("textarea[name='content']")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Comment" })).ToBeVisibleAsync();
    }



    [Test]
    public async Task IndividualPage_UserCanTypeCommentText()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        var commentBox = Page.Locator("textarea[name='content']");
        await commentBox.FillAsync("This recipe was amazing — I will definitely make it again!");

        var text = await commentBox.InputValueAsync();
        Assert.That(text, Is.EqualTo("This recipe was amazing — I will definitely make it again!"));

    }



    [Test]
    public async Task IndividualPage_UserCanSubmitComment()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        await Page.FillAsync("textarea[name='content']", "Loved this recipe, the flavors were incredible!");
        await Page.ClickAsync("button[type='submit']");

        // Wait briefly for message
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Expect either a success message or that the comment appears again
        await Expect(Page.Locator("body")).ToContainTextAsync("Loved this recipe");
    }
    


    [Test]
    public async Task IndividualPage_ShowsValidationError_WhenCommentIsEmpty()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        await Page.WaitForSelectorAsync("button[type='submit']");
        await Page.ClickAsync("button[type='submit']");

    // Browser’s HTML5 required validation should prevent submission
        var isValid = await Page.EvaluateAsync<bool>("document.querySelector('textarea[name=\"content\"]').checkValidity()");
        Assert.That(isValid, Is.False,"Textarea should be required and invalid when empty.");
}

}




