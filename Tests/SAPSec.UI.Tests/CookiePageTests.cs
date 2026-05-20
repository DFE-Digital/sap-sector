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
    public async Task CookiesPage_SaveAcceptedCookies_ShowsSuccessBannerAndKeepsReturnLink()
    {
        await Page.GotoAsync("/accessibility");
        await Page.Locator(".govuk-footer__link[href='/cookies']").ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await Page.GetByLabel("Yes, use additional cookies").CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();

        await Page.WaitForURLAsync("**/cookies");

        var banner = Page.Locator("#cookie-settings-success-banner");
        await Expect(banner).ToBeVisibleAsync();
        await Expect(banner).ToContainTextAsync("You've set your cookie preferences.");
        await Expect(banner).ToContainTextAsync("Go back to the page you were looking at");
        await Expect(Page.Locator("#cookies-banner")).Not.ToBeVisibleAsync();

        var focusedElementId = await Page.EvaluateAsync<string>("document.activeElement && document.activeElement.id");
        focusedElementId.Should().Be("cookie-settings-success-banner");

        var returnLink = banner.GetByRole(AriaRole.Link, new() { Name = "Go back to the page you were looking at" });
        (await returnLink.GetAttributeAsync("href")).Should().Be("/accessibility");
    }

    [Fact]
    public async Task CookiesPage_DoesNotRenderBackLink()
    {
        await Page.GotoAsync("/cookies?returnUrl=%2Faccessibility");

        var backLink = Page.GetByRole(AriaRole.Link, new() { Name = "Back", Exact = true });

        await Expect(backLink).ToHaveCountAsync(0);
    }
}
