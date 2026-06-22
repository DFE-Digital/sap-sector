using SAPSec.Test.Integration.Setup;
using System.Net;

namespace SAPSec.Test.Integration;

[Collection("IntegrationTestsCollection")]
public class PageTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task HomePage_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/health")]
    [InlineData("/healthcheck")]
    public async Task CommonPages_ReturnSuccess(string url)
    {
        // Act
        var response = await fixture.Client.GetAsync(url);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Response_ContainsSecurityHeaders()
    {
        // Act
        var response = await fixture.Client.GetAsync("/");
        // Assert
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        var xFrameOptions = response.Headers.GetValues("X-Frame-Options").First();
        Assert.Equal("DENY", xFrameOptions);
    }

    [Fact]
    public async Task Response_ContainsContentSecurityPolicy()
    {
        // Act
        var response = await fixture.Client.GetAsync("/");
        // Assert
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        Assert.Contains("default-src 'self'", csp);
    }
}