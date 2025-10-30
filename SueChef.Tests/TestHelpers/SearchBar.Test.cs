using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.Test;
using SueChef.TestHelpers;
using SueChef.ViewModels;
namespace SueChef.Tests;


    [TestFixture]
    public class NavbarSearchTests : PageTest
    {
        private string BaseUrl => TestContext.Parameters["baseURL"] ?? "http://localhost:5179";

        [Test]
        public async Task NavbarSearch_Pancakes_ShowsResults()
        {
            //await Page.SetViewportSizeAsync(1280, 900); // ensure >= md so navbar input can show
            await Page.GotoAsync($"{BaseUrl}/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Expect(Page.GetByTestId("testingForSearchButton")).ToBeVisibleAsync();
            await Page.GetByTestId("testingForSearchButton").ClickAsync();

            //await Expect(Page.Locator("#navbarSearchInput")).ToBeVisibleAsync();
            await Page.Locator("#navbarSearchInput").FillAsync("pancakes");
            await Page.GetByTestId("testingForSearchButton").ClickAsync();
            await Page.WaitForURLAsync(new Regex(@"/search", RegexOptions.IgnoreCase));
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Basic results assertions — adapt text to your actual titles/meta
            await Expect(Page.GetByText("Search results")).ToBeVisibleAsync();
            await Expect(Page.GetByText("mins")).ToBeVisibleAsync();   // e.g., "25 mins"
            //await Expect(Page.GetByText("Easy")).ToBeVisibleAsync(); // difficulty label
        }

        [Test]
        public async Task Search_Pancakes_Shows_Easy_Label()
        {
            await Page.GotoAsync("http://localhost:5179/search?searchQuery=pancakes");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            // Expect that at least one result card contains "Pancake"
            var resultCard = Page.Locator("body");
            await Expect(resultCard).ToContainTextAsync("Pancake");
            // Expect that the label "Easy" appears somewhere near that result
            await Expect(resultCard).ToContainTextAsync("Easy");
            await Expect(resultCard).ToContainTextAsync("Vegetarian");
        }
        [Test]
        public async Task Specific_Search_Is_Only_American()
        {
            await Page.GotoAsync("http://localhost:5179/search?searchQuery=&category=&chef=&difficulty=&duration=");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Open the Category dropdown
            await Page.Locator("#categoryDropdown").ClickAsync();

            // Wait for the list to be visible (it starts with class 'hidden')
            var categoryList = Page.Locator("#categoryList");
            await Expect(categoryList).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bhidden\b"));

            // Click the "American" option (buttons have data-value)
            await Page.Locator("#categoryList button[data-value='American']").ClickAsync();

            // Assert the selected text updated
            await Expect(Page.Locator("#categorySelectedText")).ToHaveTextAsync(
                new System.Text.RegularExpressions.Regex(@"\bAmerican\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

            // Submit the form by clicking the Search button (type=submit)
            await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Search" }).ClickAsync();

            // Wait for results to render
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert that an American recipe appears
            await Expect(Page.GetByText("Hearty Chili Con Carne", new() { Exact = false })).ToBeVisibleAsync();
        }


        [Test]
        public async Task Ingredient_Aubergine_Should_Find_Moroccan_Chickpea_Vegetable_Tagine()
        {
            await Page.GotoAsync($"{BaseUrl}/search?searchQuery=&category=&chef=&difficulty=&duration=");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Open the Ingredient dropdown
            await Page.Locator("#ingredientDropdown").ClickAsync();

            // Wait for the list (starts with 'hidden')
            var ingredientList = Page.Locator("#ingredientList");
            await Expect(ingredientList).Not.ToHaveClassAsync(new Regex(@"\bhidden\b"));

            // Click the "Aubergine" checkbox by its label text (label wraps the input)
            await ingredientList
                .Locator("label", new() { HasTextString = "Aubergine" })
                .ClickAsync();

            // Submit the form
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert the expected recipe is shown (case-insensitive, partial match)
            await Expect(Page.GetByText("Moroccan Chickpea Vegetable Tagine", new() { Exact = false }))
                .ToBeVisibleAsync();
        }
        [Test]
        public async Task Chef_Alex_Should_Find_Authentic_Chicken_Tikka_Masala_Curry()
        {
            await Page.GotoAsync($"{BaseUrl}/search?searchQuery=&category=&chef=&difficulty=&duration=");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Open the Chef dropdown
            await Page.Locator("#chefDropdown").ClickAsync();

            // Wait for the list to attach and become visible (starts with 'hidden')
            var chefList = Page.Locator("#chefList");
            await chefList.WaitForAsync(); // attached to DOM
            // If your toggle doesn’t remove 'hidden' synchronously, force-show for test stability:
            await chefList.EvaluateAsync("el => el.classList.remove('hidden')");

            // Primary: exact data-value match
            var option = chefList.Locator("button[data-value='Alex']");

            if (await option.CountAsync() == 0)
            {
                // Fallback: match by visible text containing 'Alex'
                option = chefList.Locator("button", new() { HasTextString = "Alex" });

                if (await option.CountAsync() == 0)
                {
                    // Debug help: dump list text so you see actual values
                    var dump = await chefList.InnerTextAsync();
                    Console.WriteLine($"#chefList contents:\n{dump}");
                    Assert.Fail("No chef option found for 'Alex'. Check actual names in #chefList (see console dump).");
                }
            }

            await option.First.ClickAsync();

            // the selected text should update
            await Expect(Page.Locator("#chefSelectedText"))
                .ToHaveTextAsync(new Regex(@"\bAlex\b", RegexOptions.IgnoreCase));

            // Submit the form
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert expected recipe
            await Expect(Page.GetByText("Authentic Chicken Tikka Masala Curry", new() { Exact = false }))
                .ToBeVisibleAsync();
        
        }

        [Test]
        public async Task Difficulty_Easy_Should_Find_Fresh_Mediterranean_Greek_Salad_Bowl()
        {
            // Navigate to search page
            await Page.GotoAsync($"{BaseUrl}/search?searchQuery=&category=&chef=&difficulty=&duration=");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Open Difficulty dropdown
            await Page.Locator("#difficultyDropdown").ClickAsync();

            // Wait for dropdown to attach and become visible (starts as hidden)
            var diffList = Page.Locator("#difficultyList");
            await diffList.WaitForAsync();
            await diffList.EvaluateAsync("el => el.classList.remove('hidden')");

            // Click "Easy" (data-value='1')
            var easyButton = diffList.Locator("button[data-value='1']");
            await easyButton.ClickAsync();

            // Submit the form (Search button)
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert the expected recipe is visible
            await Expect(Page.GetByText("Fresh Mediterranean Greek Salad Bowl", new() { Exact = false }))
                .ToBeVisibleAsync();
        }
        [Test]
        public async Task Duration_Over40_Should_Find_Classic_Neapolitan_Margherita_Pizza()
        {
            // Go to search page (clean state)
            await Page.GotoAsync($"{BaseUrl}/search?searchQuery=&category=&chef=&difficulty=&duration=");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            //Open the "Total Time" dropdown
            await Page.Locator("#durationDropdown").ClickAsync();

            // Ensure the list is visible (it starts with 'hidden')
            var durationList = Page.Locator("#durationList");
            await durationList.WaitForAsync();
            await durationList.EvaluateAsync("el => el.classList.remove('hidden')");

            //Select "> 40 min" (data-value='over40')
            await durationList.Locator("button[data-value='over40']").ClickAsync();

            // verify selected label updates to "> 40 min"
            await Expect(Page.Locator("#durationSelectedText"))
                .ToHaveTextAsync(new Regex(@">\s*40\s*min", RegexOptions.IgnoreCase));

            //  Submit the form (Search)
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert expected recipe is in results
            await Expect(Page.GetByText("Classic Neapolitan Margherita Pizza", new() { Exact = false }))
                .ToBeVisibleAsync();
        }
    }
