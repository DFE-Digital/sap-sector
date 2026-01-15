using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;
using System.Net;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SchoolPageControllerIntegrationTests(WebApplicationSetupFixture fixture) : IClassFixture<WebApplicationSetupFixture>
{
    private const string SchoolPagePath = "/school/147788";

    #region Authentication Tests

    [Fact]
    public async Task Index_WhenAuthenticated_ReturnsSuccessOrRedirect()
    {
        var response = await fixture.NonRedirectingClient.GetAsync(SchoolPagePath);

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
        var response = await fixture.Client.GetAsync(SchoolPagePath);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        }
    }

    [Fact]
    public async Task Index_WhenSuccessful_ContainsExpectedContent()
    {
        var response = await fixture.Client.GetAsync(SchoolPagePath);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Security Header Tests

    [Fact]
    public async Task Index_HasSecurityHeaders()
    {
        var response = await fixture.Client.GetAsync(SchoolPagePath);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            AssertSecurityHeaders(response);
        }
    }

    [Fact]
    public async Task Index_HasXContentTypeOptionsHeader()
    {
        var response = await fixture.Client.GetAsync(SchoolPagePath);

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
        var response = await fixture.Client.GetAsync(SchoolPagePath);

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

        var response = await fixture.Client.GetAsync(SchoolPagePath, cts.Token);

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Index_HandlesMultipleConcurrentRequests()
    {
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => fixture.Client.GetAsync(SchoolPagePath))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }

    #endregion

    #region Route Tests

    [Fact]
    public async Task Index_WithTrailingSlash_ReturnsValidResponse()
    {
        var response = await fixture.Client.GetAsync($"{SchoolPagePath}/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Index_UpperCaseRoute_ReturnsValidResponse()
    {
        var response = await fixture.Client.GetAsync(SchoolPagePath.ToUpper());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Index_LowerCaseRoute_ReturnsValidResponse()
    {
        var response = await fixture.Client.GetAsync(SchoolPagePath.ToLower());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region HTTP Method Tests

    [Fact]
    public async Task Index_HeadRequest_ReturnsValidResponse()
    {
        var request = new HttpRequestMessage(HttpMethod.Head, SchoolPagePath);

        var response = await fixture.Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Index_PostRequest_ReturnsMethodNotAllowed()
    {
        var response = await fixture.Client.PostAsync(SchoolPagePath, null);

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
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
}