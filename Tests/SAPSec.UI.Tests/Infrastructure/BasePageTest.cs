using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using SAPSec.UI.Tests.Infrastructure;

namespace SAPSec.UI.Tests.Deprecated.Infrastructure;

public abstract class BasePageTest : PageTest
{
    private readonly WebApplicationSetupFixture _fixture;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected BasePageTest(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;

        // Run in headed mode when debugging
        if (System.Diagnostics.Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("HEADED", "1");
        }
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = _fixture.BaseUrl.TrimEnd('/'),
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1280, Height = 720 },
            Locale = "en-GB",
            TimezoneId = "Europe/London",
            JavaScriptEnabled = true,
        };
    }

    // 1x1 transparent PNG, used to stub map tile responses below.
    private static readonly byte[] StubTilePng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Page.SetDefaultTimeout((float)TimeSpan.FromSeconds(60).TotalMilliseconds);
        Page.SetDefaultNavigationTimeout((float)TimeSpan.FromSeconds(100).TotalMilliseconds);

        // Search results pages render a Leaflet map that fetches tiles from
        // tile.openstreetmap.org. That server actively rate-limits/blocks automated
        // traffic (including CI runners), so real tile requests can hang indefinitely -
        // which in turn stalls every WaitForLoadStateAsync(NetworkIdle) call in this
        // suite. Stub tile responses so they resolve instantly and never block network-idle.
        await Page.RouteAsync("**/tile.openstreetmap.org/**", async route =>
        {
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "image/png",
                BodyBytes = StubTilePng
            });
        });
    }

    public async Task WaitForSearchInputsAsync(int timeoutMs = 5000)
    {
        var selector = "input[name='__Query'], input[name='Query'][type='hidden'], input[name='Query']";
        await Page.WaitForSelectorAsync(selector, new() { Timeout = timeoutMs });
        await Page.WaitForTimeoutAsync(100);
    }
    public async Task<ILocator> GetQueryInputLocatorAsync(int checkTimeoutMs = 1000)
    {
        var jsLocator = Page.Locator("input[name='__Query']");
        try
        {
            if (await jsLocator.CountAsync() > 0)
            {
                var isVisible = await jsLocator.IsVisibleAsync();
                if (isVisible) return jsLocator;
            }

            var serverLocator = Page.Locator("input[name='Query']");
            if (await serverLocator.CountAsync() > 0) return serverLocator;

            var found = await Page.WaitForSelectorAsync("input[name='__Query'], input[name='Query']", new() { Timeout = checkTimeoutMs });
            if (found != null)
            {
                var nameAttr = await found.GetAttributeAsync("name");
                if (nameAttr == "__Query")
                    return Page.Locator("input[name='__Query']");
                return Page.Locator("input[name='Query']");
            }
            return Page.Locator("input[name='Query']");
        }
        catch
        {
            return Page.Locator("input[name='Query']");
        }
    }
}