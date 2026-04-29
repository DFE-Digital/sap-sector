using FluentAssertions;
using Microsoft.Playwright;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class CookiePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    [Fact]
    public async Task CookiesPage_HasHomeBreadcrumb()
    {
        await Page.GotoAsync("/cookies");

        var breadcrumb = Page.Locator(".govuk-breadcrumbs__link").Filter(new() { HasText = "Home" });

        await Expect(breadcrumb).ToBeVisibleAsync();
        (await breadcrumb.GetAttributeAsync("href")).Should().Be("/find-a-school");
    }

    [Fact]
    public async Task CookiesPage_SaveAcceptedCookies_RedirectsToPreviousPageAndShowsTopBanner()
    {
        await Page.GotoAsync("/accessibility");
        await Page.Locator(".govuk-footer__link[href='/cookies']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var backLink = Page.GetByRole(AriaRole.Link, new() { Name = "Back", Exact = true });
        await Expect(backLink).ToBeVisibleAsync();
        (await backLink.GetAttributeAsync("href")).Should().Be("/accessibility");

        await Page.GetByLabel("Yes, use additional cookies").CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();

        await Page.WaitForURLAsync("**/accessibility?cookie-banner=accepted");

        var scrollY = await Page.EvaluateAsync<int>("window.scrollY");
        scrollY.Should().BeLessThan(50);

        var banner = Page.Locator("#accepted-cookies-banner");
        await Expect(banner).ToBeVisibleAsync();
        (await banner.TextContentAsync()).Should().Contain("You've accepted additional cookies.");
    }
}
