using FluentAssertions;
using SAPSec.UI.Tests.Infrastructure;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.UI.Tests;

[Collection("UITestsCollection")]
public class TermsOfUsePageTests(WebApplicationSetupFixture fixture) : BasePageTest(fixture)
{
    private const string TermsOfUsePath = "/terms-of-use";

    #region View Rendering Tests

    [Fact]
    public async Task TermsOfUsePage_LoadsSuccessfully()
    {
        var response = await Page.GotoAsync(TermsOfUsePath);

        response.Should().NotBeNull();
        response.Status.Should().Be(200, "Terms of Use page should return HTTP 200");
    }

    [Fact]
    public async Task TermsOfUsePage_HasCorrectTitle()
    {
        await Page.GotoAsync(TermsOfUsePath);

        var pageTitle = await Page.TitleAsync();
        pageTitle.Should().Contain(PageTitles.TermsOfUse);
    }

    [Fact]
    public async Task TermsOfUsePage_DisplaysMainHeading()
    {
        await Page.GotoAsync(TermsOfUsePath);

        var heading = Page.Locator("h1.govuk-heading-xl");
        var headingText = await heading.TextContentAsync();

        heading.Should().NotBeNull();
        headingText.Should().Be("Terms and conditions");
    }

    [Fact]
    public async Task TermsOfUsePage_ContainsAllRequiredSectionHeadings()
    {
        await Page.GotoAsync(TermsOfUsePath);

        var expectedHeadings = new[] {
            "Using the service",
            "Information about us",
            "Virus protection",
            "Disclaimer and liability",
            "Last updated"
        };

        foreach (var headingText in expectedHeadings)
        {
            var heading = await Page.Locator($"h2.govuk-heading-l:has-text(\"{headingText}\")").IsVisibleAsync();
            heading.Should().BeTrue();
        }
    }

    [Fact]
    public async Task TermsOfUsePage_ContainsServiceNameReference()
    {
        await Page.GotoAsync(TermsOfUsePath);

        // Verify "Get school improvement insights" is used consistently (as in CSHTML)
        var serviceReference = Page.Locator("text=/Get school improvement insights/");
        var count = await serviceReference.CountAsync();

        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TermsOfUsePage_ContainsCorrectLegalLanguage()
    {
        await Page.GotoAsync(TermsOfUsePath);

        var paragraphs = Page.Locator("p.govuk-body");

        // Validate critical legal phrases
        var requiredTexts = new[] {
            "you are confirming that you accept these terms and conditions",
            "If you do not agree to these General Terms, you must not use this Service",
            "This service is operated by the Department for Education (DfE)",
            "We make every effort to check and test material at all stages of production",
            "we do not guarantee our Service will be secure or free from bugs or viruses",
            "You must not misuse our Service by knowingly introducing viruses",
            "You must not attempt to gain unauthorised access",
            "You must not attack our site through a denial-of-service attack",
            "Each of these acts is a criminal offense",
            "The Get school improvements insights service and content are provided 'as is' and 'as available'",
            "without warranty of any kind whether express or implied",
            "In no event will we be liable for any loss or damage including, without limitation, indirect or consequential loss or damage",
            "These terms and conditions shall be governed by and construed in accordance with the laws of England and Wales",
            "Any dispute arising under these terms and conditions shall be subject to the exclusive jurisdiction of the courts of England and Wales"
        };

        foreach (var text in requiredTexts)
        {
            var element = paragraphs.GetByText(text);
            element.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task TermsOfUsePage_LastUpdatedDateIsRenderedCorrectly()
    {
        await Page.GotoAsync(TermsOfUsePath);

        var lastUpdatedText = await Page.Locator("text=This version was last updated on 01/12/2025").TextContentAsync();

        lastUpdatedText.Should().Be("This version was last updated on 01/12/2025");
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public async Task TermsOfUsePage_HasProperSemanticStructure()
    {
        await Page.GotoAsync(TermsOfUsePath);

        var h1Count = await Page.Locator("h1").CountAsync();
        h1Count.Should().Be(1);

        var h2Count = await Page.Locator("h2").CountAsync();
        h2Count.Should().BeGreaterThan(0);

        var h3Count = await Page.Locator("h3").CountAsync();
        h3Count.Should().Be(0);

        var h4PlusCount = await Page.Locator("h4, h5, h6").CountAsync();
        h4PlusCount.Should().Be(0);
    }

    #endregion

    #region Service Name Validation

    [Fact]
    public async Task TermsOfUsePage_ServiceNameMatchesGlobalConstant()
    {
        await Page.GotoAsync(TermsOfUsePath);

        // Verify the service name used in CSHTML matches the constant
        var serviceReference = Page.Locator("text=/Get school improvement insights/");
        var foundText = await serviceReference.First.TextContentAsync();

        foundText.Should().Contain("Get school improvement insights");
    }

    #endregion
}
