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
        // Arrange
        var page = await _browser!.NewPageAsync();

        // Act
        var response = await page.GotoAsync(BaseUrl);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);

        await page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_HasCorrectTitle()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);

        // Act
        var title = await page.TitleAsync();

        // Assert
        Assert.Contains("School Profile", title);

        await page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_DisplaysMainHeading()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);

        // Act
        var heading = await page.Locator("h1").TextContentAsync();

        // Assert
        Assert.NotNull(heading);
        Assert.NotEmpty(heading);

        await page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_DisplaysDfEHeader()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(BaseUrl);

        // Act
        var header = page.Locator(".dfe-header");
        var isVisible = await header.IsVisibleAsync();

        // Assert
        Assert.True(isVisible, "DfE header should be visible");

        await page.CloseAsync();
    }

    [Theory]
    [InlineData(1920, 1080)] // Desktop
    [InlineData(768, 1024)]  // Tablet
    [InlineData(375, 667)]   // Mobile
    public async Task HomePage_IsResponsive(int width, int height)
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.SetViewportSizeAsync(width, height);

        // Act
        await page.GotoAsync(BaseUrl);
        var heading = page.Locator("h1");
        var isVisible = await heading.IsVisibleAsync();

        // Assert
        Assert.True(isVisible, $"Heading should be visible at {width}x{height}");

        await page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_HasNoConsoleErrors()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        var consoleErrors = new List<string>();

        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                consoleErrors.Add(msg.Text);
            }
        };

        // Act
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Assert.Empty(consoleErrors);

        await page.CloseAsync();
    }
}