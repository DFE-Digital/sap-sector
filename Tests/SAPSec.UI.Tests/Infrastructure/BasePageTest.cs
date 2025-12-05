using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace SAPSec.UI.Tests.Infrastructure;

public abstract class BasePageTest : PageTest
{
    private readonly WebApplicationSetupFixture _fixture;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected BasePageTest(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;

        //Uncomment to run tests in headed mode
        //Environment.SetEnvironmentVariable("HEADED", "1");
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

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Page.SetDefaultTimeout((float)TimeSpan.FromSeconds(10).TotalMilliseconds);
        Page.SetDefaultNavigationTimeout((float)TimeSpan.FromSeconds(10).TotalMilliseconds);
    }
}