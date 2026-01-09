using FluentAssertions;
using SAPSec.UI.Tests.Infrastructure;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.UI.Tests.AccessibilityTests;

[Collection("UITestsCollection")]
public class AccessibilityPageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string AccessibilityPath = "/accessibility";

    #region View Rendering Tests

    [Fact]
    public async Task AccessibilityPage_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(AccessibilityPath);

        response.Should().NotBeNull();
        response.Status.Should().Be(200);
    }

    [Fact]
    public async Task AccessibilityPage_HasCorrectTitle()
    {
        await Page.GotoAsync(AccessibilityPath);

        var pageTitle = await Page.TitleAsync();
        pageTitle.Should().Contain(PageTitles.PrivacyPolicy);
    }

    [Fact]
    public async Task AccessibilityPage_DisplaysMainHeading()
    {
        await Page.GotoAsync(AccessibilityPath);

        var heading = Page.Locator("h1.govuk-heading-xl");
        var headingText = await heading.TextContentAsync();

        heading.Should().NotBeNull();
        headingText.Should().Contain("Accessibility statement");
    }

    [Fact]
    public async Task AccessibilityPage_DisplaysServiceName()
    {
        await Page.GotoAsync(AccessibilityPath);

        var paragraph = Page.Locator("p.govuk-body");
        var firstParagraphText = paragraph.GetByText(PageTitles.ServiceHome);

        firstParagraphText.Should().NotBeNull();
    }

    [Fact]
    public async Task AccessibilityPage_ContainsAllRequiredSections()
    {
        await Page.GotoAsync(AccessibilityPath);

        // Validate all H2 headings exist as per content
        var sectionHeadings = new[] {
            "Feedback and contact information",
            "Reporting accessibility problems with this website",
            "Enforcement procedure",
            "Technical information about this website's accessibility",
            "Compliance status",
            "Preparation of this accessibility statement"
        };

        foreach (var headingText in sectionHeadings)
        {
            var heading = Page.Locator($"h2.govuk-heading-l:has-text(\"{headingText}\")");
            heading.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task AccessibilityPage_ContainsListItemsWithExpectedContent()
    {
        await Page.GotoAsync(AccessibilityPath);

        var bulletPoints = new[] {
            "change colours, contrast levels and fonts",
            "zoom in up to 400% without the text spilling off the screen",
            "navigate most of the website using just a keyboard",
            "navigate most of the website using speech recognition software",
            "listen to most of the website using a screen reader (including the most recent versions of JAWS, NVDA and VoiceOver)"
        };

        var listItems = Page.Locator(".govuk-list--bullet li");

        for (var i = 0; i < bulletPoints.Length; i++)
        {
            var text = await listItems.Nth(i).TextContentAsync();
            text.Should().Contain(bulletPoints[i], $"List item {i + 1} should contain expected accessibility capability");
        }
    }

    [Fact]
    public async Task AccessibilityPage_ContainsExternalLinksWithCorrectAttributes()
    {
        await Page.GotoAsync(AccessibilityPath);

        var abilityNetLink = Page.Locator("a[href='https://mcmw.abilitynet.org.uk/']");
        var eassLink = Page.Locator("a[href='https://www.equalityadvisoryservice.com/']");

        // Verify links exist and have required attributes
        abilityNetLink.Should().NotBeNull();
        (await abilityNetLink.GetAttributeAsync("target")).Should().Be("_blank");
        (await abilityNetLink.GetAttributeAsync("rel")).Should().Be("noopener noreferrer");

        eassLink.Should().NotBeNull();
        (await eassLink.GetAttributeAsync("target")).Should().Be("_blank");
        (await eassLink.GetAttributeAsync("rel")).Should().Be("noopener noreferrer");

        // Verify the EASS link contains abbr for accessibility
        var abbr = eassLink.Locator("abbr[title='Equality Advisory and Support Service']");
        abbr.Should().NotBeNull();
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task AccessibilityPage_HasSemanticHTMLStructure()
    {
        await Page.GotoAsync(AccessibilityPath);

        // Ensure the main content is wrapped in the appropriate landmark
        var mainContent = Page.Locator("main");
        mainContent.Should().NotBeNull();

        // Ensure headings are hierarchical (h1 -> h2 only, no skipped levels)
        var h1Count = await Page.Locator("h1").CountAsync();
        h1Count.Should().Be(1);

        var h2Count = await Page.Locator("h2").CountAsync();
        h2Count.Should().BeGreaterThan(0);

        // Ensure no h3-h6 are used unless necessary (in this page, they shouldn't be)
        var h3Count = await Page.Locator("h3").CountAsync();
        h3Count.Should().Be(0);
    }

    #endregion
}
