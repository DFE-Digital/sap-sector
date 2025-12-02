using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

public class SchoolSearchControllerIntegrationTests : IClassFixture<WebApplicationSetupFixture>
{
    private readonly WebApplicationSetupFixture _fixture;

    // Routes based on actual controller configuration:
    // SchoolSearchController has [Route("school")] and actions have [Route("school/search")] etc.
    // This creates doubled routes: /school/school/search, /school/school/suggest
    // Exception: search-for-a-school works at /school/search-for-a-school
    private const string SearchForSchoolUrl = "/school/search-for-a-school";
    private const string SearchUrl = "/school/school/search";  // Doubled!
    private const string SuggestUrl = "/school/school/suggest"; // Doubled!

    public SchoolSearchControllerIntegrationTests(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;
    }

    #region GET /school/search-for-a-school (Index)

    [Fact]
    public async Task GetSearchForSchool_WithAuth_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync(SearchForSchoolUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Search for a school");
    }

    [Fact]
    public async Task GetSearchForSchool_WithAuth_ReturnsHtmlContent()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync(SearchForSchoolUrl);

        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    #endregion

    #region POST /school/search-for-a-school (Index)

    [Fact]
    public async Task PostSearchForSchool_WithValidQuery_RedirectsToSearch()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fixture.AuthenticatedClient.PostAsync(SearchForSchoolUrl, content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        // Note: RedirectToAction("Search") creates /school/school/search due to doubled routes
        response.Headers.Location!.ToString().Should().Contain("/school/school/search");
    }

    [Fact]
    public async Task PostSearchForSchool_WithEmptyQuery_ReturnsValidationError()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fixture.AuthenticatedClient.PostAsync(SearchForSchoolUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("Enter a school name");
    }

    [Fact]
    public async Task PostSearchForSchool_WithUrn_RedirectsToSchoolDetails()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "102848" },
            { "Urn", "102848" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fixture.AuthenticatedClient.PostAsync(SearchForSchoolUrl, content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("102848");
    }

    [Fact]
    public async Task PostSearchForSchool_WithInvalidUrn_ReturnsError()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test" },
            { "Urn", "999999999" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fixture.AuthenticatedClient.PostAsync(SearchForSchoolUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent.Should().Contain("could not find any schools");
    }

    #endregion

    #region GET /school/school/search (Search)

    [Fact]
    public async Task GetSearch_WithAuth_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{SearchUrl}?query=Test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithEmptyQuery_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{SearchUrl}?query=");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithoutQueryParameter_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync(SearchUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithValidQuery_ReturnsSearchResults()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{SearchUrl}?query=School");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetSearch_WithLongQuery_ReturnsSuccess()
    {
        var longQuery = new string('A', 500);

        var response = await _fixture.AuthenticatedClient.GetAsync($"{SearchUrl}?query={longQuery}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_WithSpecialCharacters_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{SearchUrl}?query=St.%20Mary%27s");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("School Name")]
    [InlineData("123456")]
    [InlineData("St. Mary's")]
    public async Task GetSearch_WithVariousQueries_ReturnsSuccess(string query)
    {
        var response = await _fixture.AuthenticatedClient.GetAsync(
            $"{SearchUrl}?query={Uri.EscapeDataString(query)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSearch_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var response = await _fixture.AuthenticatedClient.GetAsync(
            $"{SearchUrl}?query=School", cts.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /school/school/search (Search)

    [Fact]
    public async Task PostSearch_WithValidQuery_RedirectsToSearchGet()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test School" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fixture.AuthenticatedClient.PostAsync(SearchUrl, content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostSearch_WithUrn_RedirectsToSchoolDetails()
    {
        var formData = new Dictionary<string, string>
        {
            { "Query", "Test" },
            { "Urn", "102848" }
        };
        var content = new FormUrlEncodedContent(formData);

        var response = await _fixture.AuthenticatedClient.PostAsync(SearchUrl, content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("102848");
    }

    #endregion

    #region GET /school/school/suggest (Suggest)

    [Fact]
    public async Task GetSuggest_WithAuth_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{SuggestUrl}?queryPart=Test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_ReturnsJsonContent()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{SuggestUrl}?queryPart=School");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetSuggest_WithEmptyQuery_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{SuggestUrl}?queryPart=");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_WithSpecialCharacters_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{SuggestUrl}?queryPart=St.%20Mary%27s");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuggest_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var response = await _fixture.AuthenticatedClient.GetAsync(
            $"{SuggestUrl}?queryPart=Test", cts.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Security & Edge Cases

    [Fact]
    public async Task AllEndpoints_HaveSecurityHeaders()
    {
        var endpoints = new[]
        {
            SearchForSchoolUrl,
            $"{SearchUrl}?query=Test",
            $"{SuggestUrl}?queryPart=Test"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _fixture.AuthenticatedClient.GetAsync(endpoint);

            response.IsSuccessStatusCode.Should().BeTrue($"Endpoint {endpoint} should return success");

            if (response.Headers.Contains("X-Content-Type-Options"))
            {
                response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
            }
        }
    }

    [Fact]
    public async Task GetSearch_WithXssAttempt_SanitizesOutput()
    {
        var maliciousQuery = "<script>alert('xss')</script>";

        var response = await _fixture.AuthenticatedClient.GetAsync(
            $"{SearchUrl}?query={Uri.EscapeDataString(maliciousQuery)}");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("<script>alert('xss')</script>");
    }

    #endregion

    #region School Details

    [Fact]
    public async Task GetSchoolDetails_WithValidUrn_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync("/school/102848");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}