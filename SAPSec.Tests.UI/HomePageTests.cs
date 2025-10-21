using Microsoft.Playwright;
using Xunit;

namespace SAPSec.Tests.UI;

public class HomePageTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private readonly string _baseUrl;
    public HomePageTests()
    {
        _baseUrl = Environment.GetEnvironmentVariable("BASE_URL")
                   ?? "https://localhost:3000";
    }
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
        var response = await page.GotoAsync(_baseUrl);

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
        await page.GotoAsync(_baseUrl);

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
        await page.GotoAsync(_baseUrl);

        // Act
        var heading = await page.Locator("h1").TextContentAsync();

        // Assert
        Assert.NotNull(heading);
        Assert.NotEmpty(heading);

        await page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_DisplaysGovUkHeader()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(_baseUrl);

        // Act
        // Locate the GOV.UK header element
        var header = page.Locator("header.govuk-header");
        var isVisible = await header.IsVisibleAsync();

        // Optionally, also verify the DfE logo or link exists inside the header
        var logo = page.Locator("header.govuk-header img[alt='Department for Education']");
        var logoVisible = await logo.IsVisibleAsync();

        // Assert
        Assert.True(isVisible, "GOV.UK header should be visible");
        Assert.True(logoVisible, "DfE logo should be visible in the GOV.UK header");

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
        await page.GotoAsync(_baseUrl);
        var heading = page.Locator("h1");
        var isVisible = await heading.IsVisibleAsync();

        // Assert
        Assert.True(isVisible, $"Heading should be visible at {width}x{height}");

        await page.CloseAsync();
    }
}