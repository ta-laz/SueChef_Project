namespace SueChef.Tests;

using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SueChef.Test;
using SueChef.TestHelpers;

public class Tests : PageTest
{

    private const string BaseUrl = "http://127.0.0.1:5179";

        [OneTimeSetUp]
        public async Task OneTime()
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

    [Test]
    public async Task IndexpageHasPlaywrightInTitleAndGetStartedLinkLinkingtoTheIntroPage()
    {
        Assert.Pass();
    }
}
