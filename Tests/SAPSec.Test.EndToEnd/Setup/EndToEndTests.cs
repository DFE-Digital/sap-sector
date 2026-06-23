using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace SAPSec.Test.EndToEnd.Setup;

public abstract class EndToEndTests : PageTest
{
    private readonly EndToEndTestsFixture _fixture;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected EndToEndTests(EndToEndTestsFixture fixture)
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

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Page.SetDefaultTimeout((float)TimeSpan.FromSeconds(10).TotalMilliseconds);
        Page.SetDefaultNavigationTimeout((float)TimeSpan.FromSeconds(30).TotalMilliseconds);
    }

    protected async Task NavigateTo(string path)
    {
        var response = await Page.GotoAsync(path);

        response.Should().NotBeNull();
        response.Status.Should().Be(200);
    }

    protected async Task CurrentPageShouldNowBe(string path)
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForURLAsync($"**{path}");
    }
}