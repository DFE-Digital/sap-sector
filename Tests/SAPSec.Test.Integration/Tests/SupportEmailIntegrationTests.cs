using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;

namespace SAPSec.Test.Integration.Tests;

[Collection("JsonRepositoryIntegrationTestsCollection")]
public class SupportEmailIntegrationTests(JsonRepositoryIntegrationTestFixture fixture)
{
    [Fact]
    public async Task Footer_DisplaysSupportEmailAsMailtoLink()
    {
        var page = await fixture.RequestPageAsync(Routes.FindASchool());

        AssertSupportEmailLink(page, $"a.govuk-footer__link[href='mailto:{LayoutConstants.SupportEmail}']");
    }

    [Fact]
    public async Task NotFoundPage_DisplaysSupportEmailAsMailtoLink()
    {
        var page = await fixture.RequestPageAsync("/error/404");

        AssertSupportEmailLink(page, $"main a[href='mailto:{LayoutConstants.SupportEmail}']");
    }

    [Fact]
    public async Task FindASchoolNoResults_DisplaysSupportEmailAsMailtoLink()
    {
        const string query = "NoMatchingSchoolForSupportEmailTest";
        var page = await fixture.RequestPageAsync(Routes.FindASchool(query));

        page.QuerySelector(".app-no-results h2")!.TextContent.Should().Contain("No results found");
        AssertSupportEmailLink(page, $".app-no-results a[href='mailto:{LayoutConstants.SupportEmail}']");
    }

    [Fact]
    public async Task AccessibilityStatement_DisplaysSupportEmailInBothContactSections()
    {
        var page = await fixture.RequestPageAsync("/accessibility");
        var supportLinks = page.QuerySelectorAll($"main a[href='mailto:{LayoutConstants.SupportEmail}']");
        supportLinks.Should().HaveCount(2);
        supportLinks.Select(link => link.TextContent.Trim()).Should().Equal(
            LayoutConstants.SupportEmail,
            LayoutConstants.SupportEmail);
    }

    private static void AssertSupportEmailLink(IDocument page, string selector)
    {
        var supportLink = page.QuerySelector(selector);

        supportLink.Should().NotBeNull();
        supportLink!.TextContent.Trim().Should().Be(LayoutConstants.SupportEmail);
    }
}
