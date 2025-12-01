using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

public class StaticContentControllerIntegrationTests(WebApplicationSetupFixture fixture) : IClassFixture<WebApplicationSetupFixture>
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
}