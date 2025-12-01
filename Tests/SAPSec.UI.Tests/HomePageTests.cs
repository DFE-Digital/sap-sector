using FluentAssertions;
using SAPSec.UI.Tests.Infrastructure;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class HomePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    [Fact]
    public async Task HomePage_LoadsSuccessfully()
    {
        // Act - Use Page property from PageTest with relative URL
        var response = await Page.GotoAsync("/");

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be(200);
    }

    [Fact]
    public async Task HomePage_HasCorrectTitle()
    {
        // Arrange
        await Page.GotoAsync("/");

        // Act
        var title = await Page.TitleAsync();

        // Assert
        title.Should().Contain("School Profile");
    }

    [Fact]
    public async Task HomePage_DisplaysMainHeading()
    {
        // Arrange
        await Page.GotoAsync("/");

        // Act
        var heading = await Page.Locator("h1").TextContentAsync();

        // Assert
        heading.Should().NotBeNull();
        heading.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task HomePage_DisplaysGovUkHeader()
    {
        // Arrange
        await Page.GotoAsync("/");

        // Act
        // Locate the GOV.UK header element
        var header = Page.Locator("header.govuk-header");
        var isVisible = await header.IsVisibleAsync();

        // Optionally, also verify the DfE logo or link exists inside the header
        var logo = Page.Locator("header.govuk-header img[alt='Department for Education']");
        var logoVisible = await logo.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue("GOV.UK header should be visible");
        logoVisible.Should().BeTrue("DfE logo should be visible in the GOV.UK header");
    }

    [Theory]
    [InlineData(1920, 1080)] // Desktop
    [InlineData(768, 1024)]  // Tablet
    [InlineData(375, 667)]   // Mobile
    public async Task HomePage_IsResponsive(int width, int height)
    {
        // Arrange
        await Page.SetViewportSizeAsync(width, height);

        // Act
        await Page.GotoAsync("/");
        var heading = Page.Locator("h1");
        var isVisible = await heading.IsVisibleAsync();

        // Assert
        isVisible.Should().BeTrue($"Heading should be visible at {width}x{height}");
    }
}