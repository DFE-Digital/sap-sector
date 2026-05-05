using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class StaticContentControllerIntegrationTests(WebApplicationSetupFixture fixture)
{
    [Fact]
    public async Task GetAccessibility_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/accessibility");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        content.Should().Contain($"{SAPSec.Web.Constants.PageTitles.AccessibilityStatement} - {SAPSec.Web.Constants.LayoutConstants.ServiceName} - GOV.UK");
        content.Should().Contain("<a class=\"govuk-breadcrumbs__link\" href=\"/find-a-school\">Home</a>");
        content.Should().Contain("<a href=\"https://mcmw.abilitynet.org.uk/\" class=\"govuk-link\" target=\"_blank\" rel=\"noopener noreferrer\">AbilityNet</a>");
        content.Should().Contain("<a class=\"govuk-link\" rel=\"noopener noreferrer\" href=\"https://www.equalityadvisoryservice.com/\" target=\"_blank\">contact the Equality Advisory and Support Service");
    }

    [Fact]
    public async Task GetTermsOfUse_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/terms-of-use");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        content.Should().Contain("<a class=\"govuk-breadcrumbs__link\" href=\"/find-a-school\">Home</a>");
    }

    [Fact]
    public async Task GetCookies_WithLocalReturnUrl_DoesNotRenderBackLink()
    {
        var response = await fixture.Client.GetAsync("/cookies?returnUrl=%2Faccessibility");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("<a class=\"govuk-breadcrumbs__link\" href=\"/find-a-school\">Home</a>");
        content.Should().NotContain("class=\"govuk-back-link\"");
    }

    [Fact]
    public async Task GetCookies_WithExternalReturnUrl_DoesNotRenderBackLink()
    {
        var response = await fixture.Client.GetAsync("/cookies?returnUrl=https%3A%2F%2Fexample.com%2F");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("https://example.com/");
        content.Should().NotContain("class=\"govuk-back-link\"");
    }

    [Fact]
    public async Task GetCookies_DoesNotListRetiredApplicationInsightsCookies()
    {
        var response = await fixture.Client.GetAsync("/cookies");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("ai_session");
        content.Should().NotContain("ai_user");
    }

    [Fact]
    public async Task GetCookies_ListsOnlySupportedAdditionalCookies()
    {
        var response = await fixture.Client.GetAsync("/cookies");
        var content = await response.Content.ReadAsStringAsync();
        var googleAnalyticsTable = ExtractTable(content, "Google Analytics cookies");
        var clarityTable = ExtractTable(content, "Microsoft Clarity cookies");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        googleAnalyticsTable.Should().Contain("_ga");
        googleAnalyticsTable.Should().Contain("_ga_&lt;id&gt;");
        googleAnalyticsTable.Should().NotContain("_gid");
        googleAnalyticsTable.Should().NotContain("_dc_gtm_&lt;id&gt;");

        clarityTable.Should().Contain("_clck");
        clarityTable.Should().Contain("_clsk");
        clarityTable.Should().NotContain("CLID");
        clarityTable.Should().NotContain("ANONCHK");
        clarityTable.Should().NotContain("MR");
        clarityTable.Should().NotContain("MUID");
        clarityTable.Should().NotContain("SM");
    }

    [Fact]
    public async Task GetCookies_WithAcceptedBannerQuery_RendersAcceptedCookieBanner()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/cookies?cookie-banner=accepted");
        request.Headers.Add("Cookie", "cookie_policy=enabled");

        var response = await fixture.Client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("You've accepted additional cookies.");
        content.Should().Contain("id=\"cookies-banner\"");
    }

    [Fact]
    public async Task GetCookies_WithRejectedBannerQuery_RendersRejectedCookieBanner()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/cookies?cookie-banner=rejected");
        request.Headers.Add("Cookie", "cookie_policy=disabled");

        var response = await fixture.Client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("You've rejected additional cookies.");
        content.Should().Contain("id=\"cookies-banner\"");
    }

    private static string ExtractTable(string content, string caption)
    {
        var captionMarkup = $"<caption class=\"govuk-table__caption govuk-visually-hidden\">{caption}</caption>";
        var tableStart = content.IndexOf(captionMarkup, StringComparison.Ordinal);
        tableStart.Should().BeGreaterThanOrEqualTo(0);

        var tableEnd = content.IndexOf("</table>", tableStart, StringComparison.Ordinal);
        tableEnd.Should().BeGreaterThan(tableStart);

        return content.Substring(tableStart, tableEnd - tableStart);
    }
}
