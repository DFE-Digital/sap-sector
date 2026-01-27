using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;
using SAPSec.Web.Constants;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class HomeControllerTests(WebApplicationSetupFixture fixture)
{
    [Fact]
    public async Task HomePage_ReturnsHtmlWithCorrectTitle()
    {
        var response = await fixture.Client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain($"{PageTitles.ServiceHome} - {LayoutConstants.ServiceName} - GOV.UK");
    }

    [Fact]
    public async Task HomePage_HasFooterLinks()
    {
        var response = await fixture.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("<a class=\"govuk-footer__link\" href=\"/cookies\">Cookies</a>");
        content.Should().Contain("<a class=\"govuk-footer__link\" href=\"/accessibility\">Accessibility</a>");
        content.Should().Contain("<a class=\"govuk-footer__link\" href=\"/terms-of-use\">Terms of use</a>");
        content.Should().Contain("https://www.gov.uk/government/publications/privacy-information-education-providers-workforce-including-teachers/privacy-information-education-providers-workforce-including-teachers");
    }

    [Fact]
    public async Task HomePage_HasSkipLink()
    {
        var response = await fixture.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("<a href=\"#main-content\" class=\"govuk-skip-link\"");
    }

    [Fact]
    public async Task HomePage_HasCrownCopyright()
    {
        var response = await fixture.Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("© Crown copyright");
        content.Should().Contain("Open Government Licence v3.0");
    }
}