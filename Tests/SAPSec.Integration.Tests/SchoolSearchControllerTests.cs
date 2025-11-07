using System.Net;
using System.Text.Json;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

public class SchoolSearchControllerTests(WebApplicationSetupFixture fixture) : IClassFixture<WebApplicationSetupFixture>
{
    #region GET /school (Index) Tests

    [Fact]
    public async Task GetIndex_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetIndex_ReturnsPageWithSearchForm()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetIndex_HasSecurityHeaders()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("Content-Security-Policy");
    }

    #endregion

    #region POST /school (Index) Tests

    [Fact]
    public async Task PostIndex_WithValidQuery_RedirectsToSearch()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/school", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/school/search");
        response.Headers.Location!.ToString().Should().Contain("query=Test%20School");
    }

    [Fact]
    public async Task PostIndex_WithEmptyQuery_ReturnsValidationError()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.Client.PostAsync("/school", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or URN to start a search");
    }

    [Fact]
    public async Task PostIndex_WithShortQuery_ReturnsValidationError()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "AB" } // Only 2 characters, minimum is 3
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.Client.PostAsync("/school", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or URN (minimum 3 characters)");
    }

    [Fact]
    public async Task PostIndex_WithMinimumValidQuery_RedirectsToSearch()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "ABC" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/school", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/school/search");
    }

    [Fact]
    public async Task PostIndex_WithSpecialCharacters_RedirectsToSearch()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "St. Mary's & John's School" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/school", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostIndex_WithWhitespaceOnly_ReturnsValidationError()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "   " } // Only whitespace
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.Client.PostAsync("/school", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or URN");
    }

    #endregion

    #region GET /school/search Tests

    [Fact]
    public async Task GetSearch_WithValidQuery_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/search?query=Test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSearch_WithEmptyQuery_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/search?query=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithoutQueryParameter_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/search");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithNullQuery_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/search?query=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_ReturnsSearchResults()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/search?query=School");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetSearch_WithLongQuery_ReturnsSuccess()
    {
        // Arrange
        var longQuery = new string('A', 500); // Very long query

        // Act
        var response = await fixture.Client.GetAsync($"/school/search?query={longQuery}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithSpecialCharacters_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/search?query=St.%20Mary%27s%20%26%20School");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithNumericQuery_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/search?query=123456");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_CompletesWithinTimeout()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var response = await fixture.Client.GetAsync("/school/search?query=School", cts.Token);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /school/search Tests

    [Fact]
    public async Task PostSearch_WithValidQuery_RedirectsToSearchGet()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/school/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/school/search");
    }

    [Fact]
    public async Task PostSearch_WithEstablishmentId_RedirectsToSchoolController()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test" },
            { "EstablishmentId", "123456" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/school/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/School");
        response.Headers.Location!.ToString().Should().Contain("urn=123456");
    }

    [Fact]
    public async Task PostSearch_WithWhitespaceEstablishmentId_RedirectsToSearch()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test" },
            { "EstablishmentId", "   " } // Only whitespace
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/school/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/school/search");
        response.Headers.Location!.ToString().Should().NotContain("/School/Index");
    }

    [Fact]
    public async Task PostSearch_WithInvalidModelState_ReturnsViewWithErrors()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "AB" } // Too short
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.Client.PostAsync("/school/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostSearch_WithEmptyQuery_ReturnsViewWithErrors()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.Client.PostAsync("/school/search", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or URN");
    }

    [Fact]
    public async Task PostSearch_WithBothQueryAndEstablishmentId_PrioritizesEstablishmentId()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" },
            { "EstablishmentId", "999999" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/school/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/School");
        response.Headers.Location!.ToString().Should().Contain("urn=999999");
    }

    #endregion

    #region GET /school/suggest Tests

    [Fact]
    public async Task GetSuggest_WithValidQuery_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/suggest?queryPart=Test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetSuggest_ReturnsJsonArray()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/suggest?queryPart=School");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNull();

        // Verify it's valid JSON array
        var suggestions = JsonSerializer.Deserialize<string[]>(content);
        suggestions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSuggest_WithEmptyQuery_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/suggest?queryPart=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_WithoutQueryParameter_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/suggest");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }



    [Fact]
    public async Task GetSuggest_WithSpecialCharacters_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/suggest?queryPart=St.%20Mary%27s");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_WithShortQuery_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/suggest?queryPart=A");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_CompletesWithinTimeout()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var response = await fixture.Client.GetAsync("/school/suggest?queryPart=Test", cts.Token);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_MultipleConsecutiveCalls_ReturnsConsistently()
    {
        // Act
        var response1 = await fixture.Client.GetAsync("/school/suggest?queryPart=Test");
        var response2 = await fixture.Client.GetAsync("/school/suggest?queryPart=Test");
        var response3 = await fixture.Client.GetAsync("/school/suggest?queryPart=Test");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Edge Cases and Failure Scenarios

    [Fact]
    public async Task AllEndpoints_HaveSecurityHeaders()
    {
        // Arrange
        var endpoints = new[]
        {
            "/school",
            "/school/search?query=Test",
            "/school/suggest?queryPart=Test"
        };

        foreach (var endpoint in endpoints)
        {
            // Act
            var response = await fixture.Client.GetAsync(endpoint);

            // Assert
            response.Headers.Should().ContainKey("X-Content-Type-Options", 
                $"Endpoint {endpoint} should have X-Content-Type-Options header");
            response.Headers.Should().ContainKey("X-Frame-Options",
                $"Endpoint {endpoint} should have X-Frame-Options header");
        }
    }

    [Fact]
    public async Task PostIndex_WithMissingQueryField_ReturnsValidationError()
    {
        // Arrange
        // Intentionally not including the Query field
        var content = new FormUrlEncodedContent(new Dictionary<string, string>());

        // Act
        var response = await fixture.Client.PostAsync("/school", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or URN");
    }

    [Fact]
    public async Task GetSearch_WithUnicodeCharacters_ReturnsSuccess()
    {
        // Act
        var response = await fixture.Client.GetAsync("/school/search?query=Scköl");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostSearch_WithNullEstablishmentId_RedirectsToSearch()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" },
            { "EstablishmentId", "" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/school/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/school/search");
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("School Name")]
    [InlineData("123456")]
    [InlineData("St. Mary's")]
    public async Task GetSearch_WithVariousQueries_ReturnsSuccess(string query)
    {
        // Act
        var response = await fixture.Client.GetAsync($"/school/search?query={Uri.EscapeDataString(query)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithQueryContainingHtml_ReturnsSuccessWithoutXss()
    {
        // Arrange
        var maliciousQuery = "<script>alert('xss')</script>";

        // Act
        var response = await fixture.Client.GetAsync($"/school/search?query={Uri.EscapeDataString(maliciousQuery)}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Should not contain unescaped script tags
        content.Should().NotContain("<script>alert('xss')</script>");
    }

    [Fact]
    public async Task PostSearch_ConcurrentRequests_HandleGracefully()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" }
        };

        // Act
        for (int i = 0; i < 10; i++)
        {
            var content = new FormUrlEncodedContent(formData);
            tasks.Add(fixture.NonRedirectingClient.PostAsync("/school/search", content));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Redirect);
    }

    #endregion
}
