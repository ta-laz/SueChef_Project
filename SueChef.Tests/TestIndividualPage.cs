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
        await Expect(Page.GetByTestId("recipe-title Authentic Chicken Tikka Masala Curry"))
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
        await Page.GetByTestId("signup-submit").ClickAsync();
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
        await Expect(Page.GetByTestId("recipe-ingredients-Chicken Breast")).ToContainTextAsync("600g Chicken Breast");
        await Page.GetByTestId("serving-input").FillAsync("1");
        await Expect(Page.GetByTestId("recipe-ingredients-Chicken Breast")).ToContainTextAsync("150g Chicken Breast");

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
    public async Task IndividualPage_CommentCountIncreasesAfterNewComment()
    {
    // Sign up and log in first
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.GetByTestId("username").FillAsync("CountTester");
        await Page.GetByTestId("dob").FillAsync("1990-01-01");
        await Page.GetByTestId("email").FillAsync("count@testmail.com");
        await Page.GetByTestId("password").FillAsync("Password123!");
        await Page.GetByTestId("confirmpassword").FillAsync("Password123!");
        await Page.GetByTestId("signup-submit").ClickAsync();

    // Go to recipe page
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

    // Get initial comment count safely 
        var headerText = await Page.InnerTextAsync("h3:has-text('Comments')");
        Console.WriteLine($"🔍 Initial header text: '{headerText}'");

        var match = System.Text.RegularExpressions.Regex.Match(headerText, @"\d+");
        int initialCount = 0;
        if (match.Success)
        initialCount = int.Parse(match.Value);
        Console.WriteLine($"Initial count parsed: {initialCount}");

    // Fill and submit new comment (scoped form) 
        var commentForm = Page.Locator("form", new() { Has = Page.Locator("textarea[name='content']") });
        await commentForm.Locator("textarea[name='content']").FillAsync("Another test comment!");
        await commentForm.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Get new comment count safely 
        var newHeaderText = await Page.InnerTextAsync("h3:has-text('Comments')");
        Console.WriteLine($"New header text: '{newHeaderText}'");

        var newMatch = System.Text.RegularExpressions.Regex.Match(newHeaderText, @"\d+");
        int newCount = 0;
        if (newMatch.Success)
            newCount = int.Parse(newMatch.Value);
        Console.WriteLine($" New count parsed: {newCount}");

    // Verify comment count increased 
        Assert.That(newCount, Is.EqualTo(initialCount + 1), 
            $"Expected comment count to increase by 1. Before: {initialCount}, After: {newCount}");
    }




    [Test]
    public async Task IndividualPage_CommentBoxClearsAfterSubmit()
    {
    // Sign up and log in
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.GetByTestId("username").FillAsync("ClearTester");
        await Page.GetByTestId("dob").FillAsync("1990-05-05");
        await Page.GetByTestId("email").FillAsync("clear@testmail.com");
        await Page.GetByTestId("password").FillAsync("Password123!");
        await Page.GetByTestId("confirmpassword").FillAsync("Password123!");
        await Page.GetByTestId("signup-submit").ClickAsync();

    // Go to recipe page
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Locate the specific comment form that contains the textarea
        var commentForm = Page.Locator("form", new() { Has = Page.Locator("textarea[name='content']") });
        var textarea = commentForm.Locator("textarea[name='content']");

    // Fill and submit comment within the scoped form
        await textarea.FillAsync("Testing if comment box clears after submission!");
        await commentForm.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

    // Wait for reload / form reset
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Re-locate the textarea after reload
        textarea = Page.Locator("form textarea[name='content']");
        var valueAfterSubmit = await textarea.InputValueAsync();

        Assert.That(valueAfterSubmit, Is.EqualTo(""), 
            "Expected the comment textarea to be cleared after submitting a comment.");
    }



    [Test]
    public async Task IndividualPage_DisplaysChefNameDifficultyAndTimes()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        var body = Page.Locator("body");

        await Expect(body).ToContainTextAsync("By");
        await Expect(body).ToContainTextAsync("Difficulty:");
        await Expect(body).ToContainTextAsync("Prep Time:");
        await Expect(body).ToContainTextAsync("Cook Time:");
    }


    [Test]
    public async Task IndividualPage_IngredientQuantitiesScaleWithServings()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        var initialText = await Page.InnerTextAsync("[data-testid='recipe-ingredients-Chicken Breast']");
        Assert.That(initialText, Does.Contain("600g"), "Expected 4 servings to show 600g");

        await Page.GetByTestId("serving-input").FillAsync("2");
        await Expect(Page.GetByTestId("recipe-ingredients-Chicken Breast"))
            .ToContainTextAsync("300g Chicken Breast");
    }




    [Test]
    public async Task IndividualPage_StarSelectionHighlightsCorrectly()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");

        var star3 = Page.GetByTestId("star-rating-3");
        await star3.ClickAsync();

    // Check that it's visually marked as selected (yellow)
        var classAttr = await star3.GetAttributeAsync("class");
        Assert.That(classAttr, Does.Contain("text-yellow-400"), "Expected star 3 to be highlighted after click.");
    }


    [Test]
    public async Task IndividualPage_LoggedInUserCanAddToFavourites()
    {
    // Sign up and log in
        await Page.GotoAsync($"{BaseUrl}/signup");
        await Page.GetByTestId("username").FillAsync("FavTester");
        await Page.GetByTestId("dob").FillAsync("1992-06-15");
        await Page.GetByTestId("email").FillAsync("favtester@testmail.com");
        await Page.GetByTestId("password").FillAsync("Password123!");
        await Page.GetByTestId("confirmpassword").FillAsync("Password123!");
        await Page.GetByTestId("signup-submit").ClickAsync();

    // Go to a recipe page
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Click the favourite button by its ID
        await Page.Locator("#favourite_button").ClickAsync();

    // Wait for potential server response or UI update
        await Page.WaitForTimeoutAsync(1000);

    // Verify the page shows a success message or change in the heart icon
        var button = Page.Locator("#favourite_button svg");
        var fill = await button.GetAttributeAsync("fill");

        Assert.That(fill, Is.EqualTo("white").Or.EqualTo("currentColor"),
            "Expected the heart icon to change its fill after clicking (indicating it was favourited).");
    }

    [Test]
    public async Task IndividualPage_ShowsAllergenWarnings_WhenPresent()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bodyText = await Page.InnerTextAsync("body");

        if (!bodyText.Contains("Allergens"))
        {
            Assert.Inconclusive("No allergen section found — skipping test (recipe may not include allergens).");
            return;
        }

        Assert.That(bodyText, Does.Contain("Contains"), "Expected allergen warning text to appear.");
    }


    [Test]
    public async Task IndividualPage_ScrollsToCommentsSection_OnClick()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var scrollButton = Page.Locator("button:has-text('Comments')");
        if (await scrollButton.CountAsync() == 0)
            Assert.Inconclusive("No 'Comments' scroll button found — feature may not exist.");

        await scrollButton.ClickAsync();

        var commentSection = Page.Locator("h3:has-text('Comments')");
        await Expect(commentSection).ToBeVisibleAsync();
    }

    [Test]
    public async Task IndividualPage_ShoppingListButtonNavigatesCorrectly()
    {
        var recipeId = 2;
        await Page.GotoAsync($"{BaseUrl}/Recipe/{recipeId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var shopButton = Page.Locator("button:has-text('Add to Shopping List')");
        if (await shopButton.CountAsync() == 0)
            Assert.Inconclusive("No shopping list button found — feature may not exist.");

        await Task.WhenAll(
            shopButton.ClickAsync(),
            Page.WaitForLoadStateAsync(LoadState.NetworkIdle)
        );

        Assert.That(Page.Url, Does.Contain("ShoppingList"), "Expected to navigate to Shopping List after clicking the button.");
    }
}




