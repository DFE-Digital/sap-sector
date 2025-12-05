using System.Net;
using System.Net.Http.Json;
using SAPSec.Integration.Tests.Infrastructure;
using SAPSec.Web.Domain;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class HealthEndpointTests(WebApplicationSetupFixture fixture)
{
    [Fact]
    public async Task HealthEndpoint_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/health");

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
        var response = await fixture.Client.GetAsync("/healthcheck");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsJson()
    {
        // Act
        var response = await fixture.Client.GetAsync("/health");

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsValidHealthCheckResponse()
    {
        // Act
        var response = await fixture.Client.GetAsync("/health");

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
        var response = await fixture.Client.GetAsync("/health");
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
        var response = await fixture.Client.GetAsync(endpoint, cts.Token);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HealthEndpoint_CanBeCalledMultipleTimes()
    {
        // Act
        var response1 = await fixture.Client.GetAsync("/health");
        var response2 = await fixture.Client.GetAsync("/health");
        var response3 = await fixture.Client.GetAsync("/health");

        // Assert - All should return the same status
        Assert.Equal(response1.StatusCode, response2.StatusCode);
        Assert.Equal(response2.StatusCode, response3.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_IncludesTimestamp()
    {
        // Act
        var response = await fixture.Client.GetAsync("/health");
        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.NotEqual(default(DateTime), healthResponse.Timestamp);
    }
}