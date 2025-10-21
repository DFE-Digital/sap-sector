using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Web;
using SAPSec.Web.Controllers;
using Xunit;

namespace SAPSec.Tests.Integration;

public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert - Accept both 200 (healthy) and 500 (unhealthy but responding)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.InternalServerError,
            $"Expected 200 or 500, got {response.StatusCode}"
        );
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
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsValidHealthCheckResponse()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Verify successful response first
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.InternalServerError,
            $"Expected 200 or 500, got {response.StatusCode}"
        );

        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.NotNull(healthResponse.Status);
        Assert.True(
            healthResponse.Status == "Healthy" || healthResponse.Status == "Unhealthy",
            $"Status should be Healthy or Unhealthy, got: {healthResponse.Status}"
        );
        Assert.NotEmpty(healthResponse.Checks);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsExpectedChecks()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.NotEmpty(healthResponse.Checks);

        // Check that expected health checks are present
        var checkNames = healthResponse.Checks.Select(c => c.Name).ToList();
        Assert.Contains("ApplicationRunning", checkNames);
        Assert.Contains("StaticFiles", checkNames);

        // ApplicationRunning should always pass
        var appCheck = healthResponse.Checks.First(c => c.Name == "ApplicationRunning");
        Assert.Equal("Pass", appCheck.Status);
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
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HealthEndpoint_CanBeCalledMultipleTimes()
    {
        // Act
        var response1 = await _client.GetAsync("/health");
        var response2 = await _client.GetAsync("/health");
        var response3 = await _client.GetAsync("/health");

        // Assert - All should return the same status
        Assert.Equal(response1.StatusCode, response2.StatusCode);
        Assert.Equal(response2.StatusCode, response3.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_IncludesTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.NotEqual(default(DateTime), healthResponse.Timestamp);
    }

    [Fact]
    public async Task HealthEndpoint_HasSecurityHeaders()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert - Check that security headers are present
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }
}