using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class CookiePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    [Fact]
    public async Task CookiesPage_SaveAcceptedCookies_ShowsTopBannerAndScrollsToTop()
    {
        await Page.GotoAsync("/cookies");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await Page.GetByLabel("Yes, use additional cookies").CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();

        await Page.WaitForURLAsync("**/cookies?cookie-banner=accepted");

        var scrollY = await Page.EvaluateAsync<int>("window.scrollY");
        scrollY.Should().BeLessThan(50);

        var banner = Page.Locator("#accepted-cookies-banner");
        await Expect(banner).ToBeVisibleAsync();
        (await banner.TextContentAsync()).Should().Contain("You've accepted additional cookies.");
    }
}
