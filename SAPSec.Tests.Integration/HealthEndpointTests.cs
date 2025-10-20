using System.Net;
using System.Net.Http.Json;
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
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheckEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/healthcheck");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsJson()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsValidHealthCheckResponse()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.Equal("Healthy", healthResponse.Status);
        Assert.NotEmpty(healthResponse.Checks);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsAllRequiredChecks()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.Contains(healthResponse.Checks, c => c.Name == "ApplicationRunning");
        Assert.Contains(healthResponse.Checks, c => c.Name == "StaticFiles");
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/healthcheck")]
    public async Task HealthEndpoints_ReturnSuccessWithinTimeout(string endpoint)
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var response = await _client.GetAsync(endpoint, cts.Token);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
}