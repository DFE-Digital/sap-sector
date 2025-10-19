using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SAPSec.Web;
using SAPSec.Web.Controllers;
using Xunit;

namespace SAPSec.Tests.Integration;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheckEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/healthcheck");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsJson()
    {
        var response = await _client.GetAsync("/health");
        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        healthResponse.Should().NotBeNull();
        healthResponse!.Status.Should().Be("Healthy");
    }
}