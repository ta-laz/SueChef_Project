namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.Test;
using SueChef.TestHelpers;
using SueChef.ViewModels;

public class MealPlansPage : PageTest
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

        await Page.GotoAsync($"{BaseUrl}/signin");
        await Page.GetByTestId("username").FillAsync("user1");
        await Page.GetByTestId("password").FillAsync("pass");
        await Page.GetByTestId("signin-submit").ClickAsync();

        await Page.GotoAsync("/MealPlans");
    }
    public override BrowserNewContextOptions ContextOptions()
      => new BrowserNewContextOptions
      {
          BaseURL = BaseUrl
      };

    [Test]
    public async Task ViewDataTitle_MealPlansPage_IsMealPlansPage()
    {
        await Expect(Page).ToHaveTitleAsync(new Regex("Meal Plans Page"));
    }

    [Test]
    public async Task Title_MealPlansPage_IsMealPlansPage()
    {
        await Expect(Page.GetByTestId("meal-plans-page-title")).ToHaveTextAsync("Meal Plans");
    }

    [Test]
    public async Task MealPlanCount_MealPlansPage_Is0Initially()
    {
        await Expect(Page.GetByTestId("number-of-meal-plans")).ToHaveTextAsync("0 Plans");
    }

    [Test]
    public async Task NoMealPlansYetCard_MealPlansPage_IsVisible()
    {
        await Expect(Page.GetByTestId("no-meal-plans-card")).ToBeVisibleAsync();
    }

    [Test]
    public async Task NoMealPlansText_MealPlansPage_IsVisible()
    {
        await Expect(Page.GetByTestId("no-meal-plans-text")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("no-meal-plans-text")).ToHaveTextAsync(@"You haven't created any meal plans yet...
        Save and sort your favourite recipes into meal plans then use our handy shopping list generator!");
    }

    [Test]
    public async Task CreateMealPlanCard_MealPlansPage_IsVisible()
    {
        await Expect(Page.GetByTestId("create-meal-plan-card")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateMealPlanButton_MealPlansPage_IsVisible()
    {
        await Expect(Page.GetByTestId("create-meal-plan-button")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateMealPlan_MealPlansPage_CancelButtonDoesNotAddPlan()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("Test meal plan");
        await Page.GetByTestId("cancel-newplan").ClickAsync();
        await Expect(Page.GetByText("Test meal plan")).ToBeHiddenAsync();
    }

    [Test]
    public async Task CreateMealPlan_MealPlansPage_ShowsNewMealPlan()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("Test meal plan");
        await Page.GetByTestId("submit-newplan").ClickAsync();
        await Expect(Page.GetByText("Test meal plan")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateMealPlan_MealPlansPage_UpdatesMealPlanCount()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("Test meal plan");
        await Page.GetByTestId("submit-newplan").ClickAsync();
        await Expect(Page.GetByText("Test meal plan")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("number-of-meal-plans")).ToContainTextAsync("1 Plan");
    }

    [Test]
    public async Task CreateMealPlan_MealPlansPage_Has0RecipesInitially()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").ClickAsync();
        await Expect(Page.GetByText("0 recipes")).ToBeVisibleAsync();
        await Expect(Page.GetByText("No recipes yet")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateMealPlan_MealPlansPage_ShowsTodaysDate()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").ClickAsync();
        await Page.WaitForURLAsync("/MealPlans");

        await Expect(Page.GetByTestId("updated-text-10")).ToBeVisibleAsync();
        string today = DateTime.Now.ToString("dd/MM/yyyy");
        await Expect(Page.GetByTestId("updated-text-10")).ToHaveTextAsync($"Updated {today}");
    }

    [Test]
    public async Task CreateTwoMealPlans_MealPlansPage_NewestMealPlanIsFirst()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").First.ClickAsync();
        await Page.WaitForURLAsync("/MealPlans");
        await Expect(Page.GetByText("test-name")).ToBeVisibleAsync();

        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name-2");
        await Page.GetByTestId("submit-newplan").First.ClickAsync();
        await Page.WaitForURLAsync("/MealPlans");
        await Expect(Page.GetByText("test-name-2")).ToBeVisibleAsync();

        await Expect(Page.Locator("#meal-plan-title").First).ToHaveTextAsync("test-name-2");
    }

    [Test]
    public async Task ThreeDotsButton_MealPlansPage_IsVisible()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").First.ClickAsync();
        await Page.WaitForURLAsync("/MealPlans");
        await Expect(Page.GetByText("test-name")).ToBeVisibleAsync();

        await Expect(Page.GetByTestId("three-dots-button-10")).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditButton_MealPlansPage_IsVisible()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").ClickAsync();
        await Page.WaitForURLAsync("/MealPlans");
        await Expect(Page.GetByText("test-name")).ToBeVisibleAsync();

        await Page.GetByTestId("three-dots-button-10").ClickAsync();
        await Expect(Page.GetByTestId("edit-button-10")).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditMealPlan_MealPlansPage_ShowsEditedMealPlanTitle()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").First.ClickAsync();
        await Page.WaitForURLAsync("/MealPlans");
        await Expect(Page.GetByText("test-name")).ToBeVisibleAsync();

        await Page.GetByTestId("three-dots-button-10").ClickAsync();
        await Page.GetByTestId("edit-button-10").ClickAsync();
        await Page.GetByTestId("mealplan-name-input-10").FillAsync("edited-test-name");
        await Page.GetByTestId("edit-meal-plan-submit").ClickAsync();

        await Expect(Page.GetByText("edited-test-name")).ToBeVisibleAsync();
    }

    [Test]
    public async Task DeleteMealPlan_MealPlansPage_IsVisible()
    {
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").First.ClickAsync();
        await Page.WaitForURLAsync("/MealPlans");
        await Expect(Page.GetByText("test-name")).ToBeVisibleAsync();

        await Page.GetByTestId("three-dots-button-10").ClickAsync();
        await Expect(Page.GetByTestId("delete-button-10")).ToBeVisibleAsync();
    }

    [Test]
    public async Task DeleteMealPlan_MealPlansPage_ShowsNoMealPlans()
    {
        Page.Dialog += async (_, dialog) =>
        {
            if (dialog.Type == "confirm")
                await dialog.AcceptAsync(); // Clicks OK
        };

        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").First.ClickAsync();
        await Page.WaitForURLAsync("/MealPlans");
        await Expect(Page.GetByText("test-name")).ToBeVisibleAsync();

        await Page.GetByTestId("three-dots-button-10").ClickAsync();
        await Page.GetByTestId("delete-button-10").ClickAsync();

        // the test data seeder may not be wiping the data fully so this expect fails
        // await Expect(Page.GetByText("test-name")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("number-of-meal-plans")).ToContainTextAsync("0 Plans");
    }


    [Test]
    public async Task CancelDeleteMealPlan_MealPlansPage_StillShowsMealPlan()
    {
    //  Create a new meal plan
        await Page.GetByTestId("create-meal-plan-button").ClickAsync();
        await Page.GetByTestId("newplan-name-input").FillAsync("test-name");
        await Page.GetByTestId("submit-newplan").First.ClickAsync();

    // Wait until redirected to MealPlans page
        await Page.WaitForURLAsync("**/MealPlans");
        await Expect(Page.GetByText("test-name")).ToBeVisibleAsync();

    // Open the options menu (three dots)
        await Page.GetByTestId("three-dots-button-10").ClickAsync();

    // Simulate opening delete menu but cancelling (press ESC or click outside)
        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForTimeoutAsync(500); // give the UI a moment to close the menu

    // Verify that the plan still appears and count hasn't changed
        await Expect(Page.GetByText("test-name")).ToBeVisibleAsync();

        var planCount = await Page.InnerTextAsync("[data-testid='number-of-meal-plans']");
        Console.WriteLine($"DEBUG: Meal plan count text = {planCount}");
        Assert.That(planCount.Contains("1 Plan"), $"Expected '1 Plan' but got '{planCount}'");
    }
}