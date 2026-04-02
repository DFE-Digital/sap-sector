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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetTermsOfUse_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/terms-of-use");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetCookies_WithAcceptedBannerQuery_RendersAcceptedCookieBanner()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/cookies?cookie-banner=accepted");
        request.Headers.Add("Cookie", "cookie_policy=enabled");

        var response = await fixture.Client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("You've accepted analytics cookies.");
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
        content.Should().Contain("You've rejected analytics cookies.");
        content.Should().Contain("id=\"cookies-banner\"");
    }
}
