using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace SAPSec.UI.Tests.Infrastructure;

public abstract class BasePageTest : PageTest
{
    private readonly WebApplicationSetupFixture _fixture;

    protected BasePageTest(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;

    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = _fixture.BaseUrl,
            IgnoreHTTPSErrors = true,
            ViewportSize = new() { Width = 1280, Height = 720 },
            Locale = "en-GB",
            TimezoneId = "Europe/London",
            JavaScriptEnabled = true
        };
    }
}