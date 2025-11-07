using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace SAPSec.UI.Tests.Infrastructure;

/// <summary>
/// Base class for Playwright page tests that configures the browser context with the base URL from the WebApplicationBrowserFixture.
/// </summary>
public abstract class BasePageTest : PageTest
{
    private readonly WebApplicationSetupFixture _fixture;

    /// <summary>
    /// Base class for Playwright page tests that configures the browser context with the base URL from the WebApplicationBrowserFixture.
    /// </summary>
    protected BasePageTest(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;

        //TODO:Uncomment to run tests in headed mode
        //Environment.SetEnvironmentVariable("HEADED", "1");
    }

    /// <summary>
    /// Override ContextOptions to inject the BaseURL from the web application fixture into the browser context.
    /// </summary>
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = _fixture.BaseUrl,
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1280, Height = 720 },
            Locale = "en-GB",
            TimezoneId = "Europe/London"
        };
    }
}