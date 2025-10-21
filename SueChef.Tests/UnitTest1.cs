namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.TestHelpers;

public class Tests : PageTest
{

    private const string BaseUrl = "http://127.0.0.1:5179";

        [OneTimeSetUp]
        public async Task OneTime()
        {
            await using var context = DbFactory.CreateTestDb();
            await TestDataSeeder.EnsureDbReadyAsync(context);
        }

        [SetUp]
        public async Task SetupDb()
    {
            await Page.GotoAsync("/");
            await using var context = DbFactory.CreateTestDb();
            await TestDataSeeder.ResetAndSeedAsync(context);
        }

    public override BrowserNewContextOptions ContextOptions()
      => new BrowserNewContextOptions
      {
          BaseURL = BaseUrl
      };
          
    // the following method is a test
    [Test]
    public async Task IndexpageHasPlaywrightInTitleAndGetStartedLinkLinkingtoTheIntroPage()
    {
        // go to the MakersBnB Index page
        // NB: update the localhost port with the one your server is running on 

        // expect the page title to contain "Index Page - MakersBnB"
        await Expect(Page).ToHaveTitleAsync(new Regex("Home Page"));
    }
}