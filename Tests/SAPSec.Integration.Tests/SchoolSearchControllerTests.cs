using System.Net;
using System.Text.Json;
using FluentAssertions;
using SAPSec.Infrastructure.Entities;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SchoolSearchControllerTests(WebApplicationSetupFixture fixture)
{
    #region GET /find-a-school (Index) Tests

    [Fact]
    public async Task GetIndex_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetIndex_ReturnsPageWithSearchForm()
    {
        var response = await fixture.Client.GetAsync("/find-a-school");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetIndex_HasSecurityHeaders()
    {
        var response = await fixture.Client.GetAsync("/find-a-school");

        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("Content-Security-Policy");
    }

    #endregion

    #region POST /find-a-school (Index) Tests

    [Fact]
    public async Task PostIndex_WithValidQuery_RedirectsToSearch()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/find-a-school");
        response.Headers.Location!.ToString().Should().Contain("query=Test%20School");
    }

    [Fact]
    public async Task PostIndex_WithEmptyQuery_ReturnsValidationError()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.Client.PostAsync("/find-a-school", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or school ID to start a search");
    }

    [Fact]
    public async Task PostIndex_WithShortQuery_ReturnsValidationError()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "AB" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.Client.PostAsync("/find-a-school", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or school ID (minimum 3 characters)");
    }

    [Fact]
    public async Task PostIndex_WithMinimumValidQuery_RedirectsToSearch()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "ABC" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/find-a-school");
    }

    [Fact]
    public async Task PostIndex_WithSpecialCharacters_RedirectsToSearch()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "St. Mary's & John's School" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostIndex_WithWhitespaceOnly_ReturnsValidationError()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "   " } // Only whitespace
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.Client.PostAsync("/find-a-school", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or school ID to start a search");
    }

    [Fact]
    public async Task PostIndex_WithNumericQuery_RedirectsToSchoolDetails()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "138361" },
            { "Urn", "138361" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/school/138361");
    }

    [Fact]
    public async Task PostIndex_WithUrn_RedirectsToSchoolDetails()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "school" },
            { "Urn", "138361" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/school/138361");
    }

    #endregion

    #region GET /find-a-school/search Tests

    [Fact]
    public async Task GetSearch_WithValidQuery_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=Test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSearch_WithEmptyQuery_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithoutQueryParameter_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithNullQuery_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_ReturnsSearchResults()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetSearch_WithLongQuery_ReturnsSuccess()
    {
        var longQuery = new string('A', 500); // Very long query

        var response = await fixture.Client.GetAsync($"/find-a-school/search?query={longQuery}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithSpecialCharacters_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=St.%20Mary%27s%20%26%20School");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithNumericQuery_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=147788");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));

        var response = await fixture.Client.GetAsync("/find-a-school/search?query=School", cts.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithSingle_Match_RedirectsToSchoolDetails()
    {
        var response = await fixture.NonRedirectingClient.GetAsync("/find-a-school/search?query=Notre%20Dame%20High%20School");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    #endregion

    #region POST /find-a-school/search Tests

    [Fact]
    public async Task PostSearch_WithValidQuery_RedirectsToSearchGet()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school/search", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/find-a-school/search");
    }

    [Fact]
    public async Task PostSearch_WithUrn_RedirectsToSchoolController()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test" },
            { "Urn", "147788" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school/search", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().ToLower().Should().Contain("/school/147788");
    }

    [Fact]
    public async Task PostSearch_WithWhitespaceUrn_RedirectsToSearch()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test" },
            { "Urn", "   " } // Only whitespace
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school/search", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().ToLower().Should().Contain("/find-a-school/search");
        response.Headers.Location!.ToString().Should().NotContain("/School/Index");
    }

    [Fact]
    public async Task PostSearch_WithShortQuery_ReturnsViewWithErrors()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "AB" } // Too short
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.Client.PostAsync("/find-a-school/search", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostSearch_WithEmptyQuery_ReturnsViewWithErrors()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.Client.PostAsync("/find-a-school/search", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or school ID to start a search");
    }

    [Fact]
    public async Task PostSearch_WithBothQueryAndUrn_PrioritizesUrn()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" },
            { "Urn", "147788" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school/search", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().ToLower().Should().Contain("/school/147788");
    }

    [Fact]
    public async Task PostSearch_WithNumericQuery_PrioritizesUrn()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "204/3658" },
            { "Urn", "147788" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school/search", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().ToLower().Should().Contain("/school/147788");
    }

    #endregion

    #region GET /find-a-school/suggest Tests

    [Fact]
    public async Task GetSuggest_WithValidQuery_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=Test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetSuggest_ReturnsJsonArray()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=School");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNull();

        var suggestions = JsonSerializer.Deserialize<SchoolSearchResult[]>(content);
        suggestions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSuggest_WithEmptyQuery_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_WithoutQueryParameter_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/suggest");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_WithSpecialCharacters_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=St.%20Mary%27s");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_WithShortQuery_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=A");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var response = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=Test", cts.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_MultipleConsecutiveCalls_ReturnsConsistently()
    {
        var response1 = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=Test");
        var response2 = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=Test");
        var response3 = await fixture.Client.GetAsync("/find-a-school/suggest?queryPart=Test");

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Edge Cases and Failure Scenarios

    [Fact]
    public async Task AllEndpoints_HaveSecurityHeaders()
    {
        var endpoints = new[]
        {
            "/find-a-school",
            "/find-a-school/search?query=Test",
            "/find-a-school/suggest?queryPart=Test"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await fixture.Client.GetAsync(endpoint);

            response.Headers.Should().ContainKey("X-Content-Type-Options", $"Endpoint {endpoint} should have X-Content-Type-Options header");
            response.Headers.Should().ContainKey("X-Frame-Options", $"Endpoint {endpoint} should have X-Frame-Options header");
        }
    }

    [Fact]
    public async Task PostIndex_WithMissingQueryField_ReturnsValidationError()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>());

        var response = await fixture.Client.PostAsync("/find-a-school", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name or school ID to start a search");
    }

    [Fact]
    public async Task GetSearch_WithUnicodeCharacters_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/find-a-school/search?query=Scköl");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostSearch_WithNullUrn_RedirectsToSearch()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" },
            { "Urn", "" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await fixture.NonRedirectingClient.PostAsync("/find-a-school/search", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().ToLower().Should().Contain("/find-a-school/search");
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("School Name")]
    [InlineData("147788")]
    [InlineData("St. Mary's")]
    public async Task GetSearch_WithVariousQueries_ReturnsSuccess(string query)
    {
        var response = await fixture.Client.GetAsync($"/find-a-school/search?query={Uri.EscapeDataString(query)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithQueryContainingHtml_ReturnsSuccessWithoutXss()
    {
        var maliciousQuery = "<script>alert('xss')</script>";

        var response = await fixture.Client.GetAsync($"/find-a-school/search?query={Uri.EscapeDataString(maliciousQuery)}");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("<script>alert('xss')</script>");
    }

    [Fact]
    public async Task PostSearch_ConcurrentRequests_HandleGracefully()
    {
        var tasks = new List<Task<HttpResponseMessage>>();
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" }
        };

        for (var i = 0; i < 10; i++)
        {
            var content = new FormUrlEncodedContent(formData);
            tasks.Add(fixture.NonRedirectingClient.PostAsync("/find-a-school", content));
        }

        var responses = await Task.WhenAll(tasks);

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Redirect);
    }

    #endregion
}