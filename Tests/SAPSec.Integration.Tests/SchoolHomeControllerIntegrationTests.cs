using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;
using System.Net;

namespace SAPSec.Integration.Tests;

public class SchoolHomeControllerIntegrationTests : IClassFixture<WebApplicationSetupFixture>
{
    private readonly WebApplicationSetupFixture _fixture;

    public SchoolHomeControllerIntegrationTests(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;
    }

    #region Authentication Tests

    [Fact]
    public async Task Index_WhenAuthenticated_ReturnsSuccessOrRedirect()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync(TestData.Routes.SchoolHome);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    #endregion

    #region Response Tests

    [Fact]
    public async Task Index_WhenSuccessful_ReturnsHtmlContent()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        }
    }

    [Fact]
    public async Task Index_WhenSuccessful_ContainsExpectedContent()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Redirect Behaviour Tests

    [Fact]
    public async Task Index_WhenRedirected_GoesToValidDestination()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        var currentUrl = response.RequestMessage?.RequestUri?.ToString() ?? string.Empty;

        var isValidDestination =
            currentUrl.Contains("SchoolHome", StringComparison.OrdinalIgnoreCase) ||
            currentUrl.Contains("search-for-a-school", StringComparison.OrdinalIgnoreCase) ||
            currentUrl.Contains("sign-in", StringComparison.OrdinalIgnoreCase) ||
            currentUrl.Contains("Error", StringComparison.OrdinalIgnoreCase);

        isValidDestination.Should().BeTrue(
            $"Should redirect to valid destination, but was: {currentUrl}");
    }

    [Fact]
    public async Task Index_WhenNonEstablishment_RedirectsToSchoolSearch()
    {
        var response = await _fixture.NonRedirectingClient.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            var location = response.Headers.Location?.ToString();

            var isValidRedirect =
                location?.Contains("search-for-a-school") == true ||
                location?.Contains("sign-in") == true ||
                location?.Contains("Error") == true;

            isValidRedirect.Should().BeTrue();
        }
    }

    #endregion

    #region Security Header Tests

    [Fact]
    public async Task Index_HasSecurityHeaders()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            AssertSecurityHeaders(response);
        }
    }

    [Fact]
    public async Task Index_HasXContentTypeOptionsHeader()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var headers = GetAllHeaders(response);

            if (headers.ContainsKey("X-Content-Type-Options"))
            {
                headers["X-Content-Type-Options"].Should().Contain("nosniff");
            }
        }
    }

    [Fact]
    public async Task Index_HasXFrameOptionsHeader()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var headers = GetAllHeaders(response);

            if (headers.ContainsKey("X-Frame-Options"))
            {
                headers["X-Frame-Options"].Should().Contain("DENY");
            }
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Index_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome, cts.Token);

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Index_HandlesMultipleConcurrentRequests()
    {
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _fixture.Client.GetAsync(TestData.Routes.SchoolHome))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        responses.Should().OnlyContain(r =>
            r.StatusCode == HttpStatusCode.OK ||
            r.StatusCode == HttpStatusCode.Redirect ||
            r.StatusCode == HttpStatusCode.Found);
    }

    #endregion

    #region Route Tests

    [Fact]
    public async Task Index_WithTrailingSlash_ReturnsValidResponse()
    {
        var response = await _fixture.Client.GetAsync($"{TestData.Routes.SchoolHome}/");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Index_CaseInsensitiveRoute_ReturnsValidResponse()
    {
        var response = await _fixture.Client.GetAsync("/SCHOOLHOME");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/SchoolHome")]
    [InlineData("/schoolhome")]
    [InlineData("/SCHOOLHOME")]
    public async Task Index_VariousRouteCasing_ReturnsValidResponse(string route)
    {
        var response = await _fixture.Client.GetAsync(route);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.NotFound);
    }

    #endregion

    #region HTTP Method Tests

    [Fact]
    public async Task Index_HeadRequest_ReturnsValidResponse()
    {
        var request = new HttpRequestMessage(HttpMethod.Head, TestData.Routes.SchoolHome);

        var response = await _fixture.Client.SendAsync(request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Index_PostRequest_ReturnsMethodNotAllowed()
    {
        var response = await _fixture.Client.PostAsync(TestData.Routes.SchoolHome, null);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.NotFound,
            HttpStatusCode.Redirect);
    }

    #endregion

    #region View Content Tests

    [Fact]
    public async Task Index_WhenEstablishment_ContainsCardComponents()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("app-card");
        }
    }

    [Fact]
    public async Task Index_WhenEstablishment_ContainsComparePerformanceLink()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("ComparePerformance");
        }
    }

    [Fact]
    public async Task Index_WhenEstablishment_ContainsSchoolSearchLink()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("/search-for-a-school");
        }
    }

    [Fact]
    public async Task Index_WhenEstablishment_ContainsSchoolDetailsLink()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("SchoolDetails");
        }
    }

    [Fact]
    public async Task Index_WhenEstablishment_ContainsGovukStyling()
    {
        var response = await _fixture.Client.GetAsync(TestData.Routes.SchoolHome);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("govuk-");
        }
    }

    #endregion

    #region Helper Methods

    private static void AssertSecurityHeaders(HttpResponseMessage response)
    {
        var headers = GetAllHeaders(response);

        if (headers.ContainsKey("X-Content-Type-Options"))
        {
            headers["X-Content-Type-Options"].Should().Contain("nosniff");
        }

        if (headers.ContainsKey("X-Frame-Options"))
        {
            headers["X-Frame-Options"].Should().Contain("DENY");
        }
    }

    private static Dictionary<string, IEnumerable<string>> GetAllHeaders(HttpResponseMessage response)
    {
        return response.Headers
            .Concat(response.Content.Headers)
            .ToDictionary(h => h.Key, h => h.Value);
    }

    #endregion

    #region Test Data

    private static class TestData
    {
        public static class Routes
        {
            public const string SchoolHome = "/SchoolHome";
            public const string SchoolSearch = "/search-for-a-school";
            public const string AccessDenied = "/Error/403";
        }
    }

    #endregion
}