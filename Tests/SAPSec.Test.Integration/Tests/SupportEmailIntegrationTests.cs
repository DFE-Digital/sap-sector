using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;

namespace SAPSec.Test.Integration.Tests;

[Collection("JsonRepositoryIntegrationTestsCollection")]
public class SupportEmailIntegrationTests(JsonRepositoryIntegrationTestFixture fixture)
{
    private static readonly HtmlParser HtmlParser = new();

    [Fact]
    public async Task Footer_DisplaysSupportEmailAsMailtoLink()
    {
        var response = await fixture.Client.GetAsync(Routes.FindASchool());
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
            
        var page = HtmlParser.ParseDocument(content);
        AssertSupportEmailLink(page, $"a.govuk-footer__link[href='mailto:{LayoutConstants.SupportEmail}']");
    }

    [Fact]
    public async Task NotFoundPage_DisplaysSupportEmailAsMailtoLink()
    {
        var response = await fixture.Client.GetAsync("/error/404");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = HtmlParser.ParseDocument(content);
        AssertSupportEmailLink(page, $"main a[href='mailto:{LayoutConstants.SupportEmail}']");
    }

    [Fact]
    public async Task FindASchoolNoResults_DisplaysSupportEmailAsMailtoLink()
    {
        const string query = "NoMatchingSchoolForSupportEmailTest";
        var response = await fixture.Client.GetAsync(Routes.FindASchool(query));
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = HtmlParser.ParseDocument(content);
        page.QuerySelector(".app-no-results h2")!.TextContent.Should().Contain("No results found");
        AssertSupportEmailLink(page, $".app-no-results a[href='mailto:{LayoutConstants.SupportEmail}']");
    }

    [Fact]
    public async Task AccessibilityStatement_DisplaysSupportEmailInBothContactSections()
    {
        var response = await fixture.Client.GetAsync("/accessibility");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = HtmlParser.ParseDocument(content);
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
