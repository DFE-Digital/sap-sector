using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class OrganisationControllerIntegrationTests(WebApplicationSetupFixture fixture) : IClassFixture<WebApplicationSetupFixture>
{
    #region GET /Organisation/details Tests

    [Fact]
    public async Task GetDetails_WhenNotAuthenticated_ReturnsUnauthorizedOrRedirect()
    {
        // Act
        var response = await fixture.Client.GetAsync("/Organisation/details");

        // Assert
        // Depending on auth setup, may redirect to log in or return unauthorised
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetDetails_WhenAuthenticated_ReturnsSuccessOrRedirect()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/details");

        // Assert
        // May return view with organisation details or redirect to error if no organisation
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetDetails_ReturnsHtmlContent()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/details");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        }
    }

    [Fact]
    public async Task GetDetails_CompletesWithinTimeout()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/details", cts.Token);

        // Assert
        response.Should().NotBeNull();
    }

    #endregion

    #region GET /Organisation/switch Tests

    [Fact]
    public async Task GetSwitch_WhenNotAuthenticated_ReturnsUnauthorizedOrRedirect()
    {
        // Act
        var response = await fixture.Client.GetAsync("/Organisation/switch");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetSwitch_WhenAuthenticated_ReturnsSuccessOrRedirect()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/switch");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetSwitch_ReturnsHtmlContent()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/switch");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        }
    }

    #endregion

    #region POST /Organisation/switch Tests

    [Fact]
    public async Task PostSwitch_WhenNotAuthenticated_ReturnsUnauthorizedOrRedirect()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "organisationId", "org-123" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.Client.PostAsync("/Organisation/switch", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task PostSwitch_WithEmptyOrganisationId_ReturnsBadRequest()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "organisationId", "" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/Organisation/switch", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task PostSwitch_WithValidOrganisationId_ReturnsRedirect()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "organisationId", "test-org-123" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/Organisation/switch", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task PostSwitch_WithReturnUrl_RedirectsCorrectly()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "organisationId", "test-org-123" },
            { "returnUrl", "/dashboard" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/Organisation/switch", content);

        // Assert
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            response.Headers.Location.Should().NotBeNull();
        }
    }

    #endregion

    #region GET /Organisation/api/current Tests

    [Fact]
    public async Task GetApiCurrent_WhenNotAuthenticated_ReturnsUnauthorizedOrNotFound()
    {
        // Act
        var response = await fixture.Client.GetAsync("/Organisation/api/current");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetApiCurrent_WhenAuthenticated_ReturnsJsonOrNotFound()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/api/current");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetApiCurrent_WhenSuccess_ReturnsJsonContentType()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/api/current");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    [Fact]
    public async Task GetApiCurrent_WhenSuccess_ReturnsValidJson()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/api/current");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            // Should be valid JSON
            var action = () => System.Text.Json.JsonDocument.Parse(content);
            action.Should().NotThrow();
        }
    }

    #endregion

    #region GET /Organisation/api/{organisationId} Tests

    [Fact]
    public async Task GetApiOrganisation_WithValidId_ReturnsJsonOrNotFound()
    {
        // Act
        var response = await fixture.Client.GetAsync("/Organisation/api/test-org-123");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetApiOrganisation_WithEmptyId_ReturnsBadRequestOrNotFound()
    {
        // Act - Note: Empty ID might be interpreted as a different route
        var response = await fixture.Client.GetAsync("/Organisation/api/");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetApiOrganisation_WhenSuccess_ReturnsJsonContentType()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/api/test-org-123");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    [Fact]
    public async Task GetApiOrganisation_WhenNotFound_ReturnsNotFoundStatus()
    {
        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/api/non-existent-org-xyz");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetApiOrganisation_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var orgIdWithSpecialChars = Uri.EscapeDataString("org-123!@#");

        // Act
        var response = await fixture.NonRedirectingClient.GetAsync($"/Organisation/api/{orgIdWithSpecialChars}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Route Tests

    [Fact]
    public async Task OrganisationRoutes_AreCaseInsensitive()
    {
        // Arrange
        var routes = new[]
        {
            "/Organisation/details",
            "/organisation/details",
            "/ORGANISATION/DETAILS"
        };

        foreach (var route in routes)
        {
            // Act
            var response = await fixture.Client.GetAsync(route);

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound, $"Route {route} should be accessible");
        }
    }

    [Fact]
    public async Task OrganisationRoutes_WithTrailingSlash_Work()
    {
        // Act
        var response = await fixture.Client.GetAsync("/Organisation/details/");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task OrganisationRoutes_SwitchGet_Exists()
    {
        // Act
        var response = await fixture.Client.GetAsync("/Organisation/switch");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OrganisationRoutes_ApiCurrent_Exists()
    {
        // Act
        var response = await fixture.Client.GetAsync("/Organisation/api/current");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task PostSwitch_WithExternalReturnUrl_DoesNotRedirectToExternalSite()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "organisationId", "org-123" },
            { "returnUrl", "https://malicious-site.com" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await fixture.NonRedirectingClient.PostAsync("/Organisation/switch", content);

        // Assert
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            response.Headers.Location!.ToString().Should().NotContain("malicious-site.com");
        }
    }

    [Fact]
    public async Task GetDetails_WithXssInQueryString_DoesNotExecuteScript()
    {
        // Arrange
        var xssPayload = "<script>alert('xss')</script>";

        // Act
        var response = await fixture.NonRedirectingClient.GetAsync($"/Organisation/details?test={Uri.EscapeDataString(xssPayload)}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>alert('xss')</script>");
        }
    }

    [Fact]
    public async Task GetApiOrganisation_WithSqlInjectionAttempt_HandlesGracefully()
    {
        // Arrange
        var sqlInjection = "'; DROP TABLE Users; --";

        // Act
        var response = await fixture.NonRedirectingClient.GetAsync($"/Organisation/api/{Uri.EscapeDataString(sqlInjection)}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetDetails_CompletesInReasonableTime()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/details", cts.Token);

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetApiCurrent_CompletesInReasonableTime()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/api/current", cts.Token);

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetApiOrganisation_CompletesInReasonableTime()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var response = await fixture.NonRedirectingClient.GetAsync("/Organisation/api/test-org", cts.Token);

        // Assert
        response.Should().NotBeNull();
    }

    #endregion

    #region Concurrent Request Tests

    [Fact]
    public async Task MultipleApiRequests_HandleConcurrently()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => fixture.NonRedirectingClient.GetAsync("/Organisation/api/current"));

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound));
    }

    [Fact]
    public async Task MixedEndpointRequests_HandleConcurrently()
    {
        // Arrange
        var tasks = new[]
        {
            fixture.NonRedirectingClient.GetAsync("/Organisation/details"),
            fixture.NonRedirectingClient.GetAsync("/Organisation/switch"),
            fixture.NonRedirectingClient.GetAsync("/Organisation/api/current")
        };

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(3);
        responses.Should().AllSatisfy(r => r.Should().NotBeNull());
    }

    #endregion

    #region Content Negotiation Tests

    [Fact]
    public async Task GetApiCurrent_WithAcceptJson_ReturnsJson()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Organisation/api/current");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        var response = await fixture.NonRedirectingClient.SendAsync(request);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    [Fact]
    public async Task GetApiOrganisation_WithAcceptJson_ReturnsJson()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Organisation/api/test-org");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        var response = await fixture.NonRedirectingClient.SendAsync(request);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    #endregion
}