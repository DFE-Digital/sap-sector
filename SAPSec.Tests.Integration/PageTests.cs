using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SAPSec.Web;
using Xunit;

namespace SAPSec.Tests.Integration;

public class PageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PageTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HomePage_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/health")]
    [InlineData("/healthcheck")]
    public async Task CommonPages_ReturnSuccess(string url)
    {
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Response_ContainsSecurityHeaders()
    {
        var response = await _client.GetAsync("/");

        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
    }
}