using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SAPSec.Tests.UI;

public class HomePageTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string BaseUrl = "http://localhost:3000";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        _playwright?.Dispose();
    }

    [Fact]
    public async Task HomePage_LoadsSuccessfully()
    {
        var page = await _browser!.NewPageAsync();
        var response = await page.GotoAsync(BaseUrl);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);

        await page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_HasCorrectTitle()
    {
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);

        var title = await page.TitleAsync();

        title.Should().Contain("School Profile");
        await page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_DisplaysMainHeading()
    {
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);

        var heading = await page.Locator("h1").TextContentAsync();

        heading.Should().NotBeNullOrEmpty();
        await page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_DisplaysDfEHeader()
    {
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);

        var header = page.Locator(".dfe-header");
        var isVisible = await header.IsVisibleAsync();

        isVisible.Should().BeTrue();
        await page.CloseAsync();
    }

    [Theory]
    [InlineData(1920, 1080)] // Desktop
    [InlineData(768, 1024)]  // Tablet
    [InlineData(375, 667)]   // Mobile
    public async Task HomePage_IsResponsive(int width, int height)
    {
        var page = await _browser!.NewPageAsync();
        await page.SetViewportSizeAsync(width, height);
        await page.GotoAsync(BaseUrl);

        var heading = page.Locator("h1");
        var isVisible = await heading.IsVisibleAsync();

        isVisible.Should().BeTrue();
        await page.CloseAsync();
    }
}