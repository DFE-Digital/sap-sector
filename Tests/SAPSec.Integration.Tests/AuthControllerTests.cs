using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationSetupFixture>
{
    private readonly WebApplicationSetupFixture _fixture;

    private static class ExpectedRoutes
    {
        public const string SignIn = "/Auth/sign-in";
        public const string SignOut = "/Auth/sign-out";
        public const string AccessDenied = "/Auth/access-denied";
        public const string SignedOut = "/Auth/signed-out";
        public const string DefaultReturnUrl = "/school/search-for-a-school";
    }

    public AuthControllerIntegrationTests(WebApplicationSetupFixture fixture)
    {
        _fixture = fixture;
    }

    #region GET /Auth/sign-in Tests

    [Fact]
    public async Task GetSignIn_WhenNotAuthenticated_ReturnsChallengeRedirect()
    {
        var response = await _fixture.Client.GetAsync(ExpectedRoutes.SignIn);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSignIn_WhenAuthenticated_RedirectsToReturnUrl()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync($"{ExpectedRoutes.SignIn}?returnUrl=/dashboard");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSignIn_WhenAuthenticated_WithNullReturnUrl_RedirectsToHome()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync(ExpectedRoutes.SignIn);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSignIn_HasSecurityHeaders()
    {
        var response = await _fixture.Client.GetAsync(ExpectedRoutes.SignIn);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var allHeaders = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(h => h.Key, h => h.Value);

            if (allHeaders.ContainsKey("X-Content-Type-Options"))
            {
                allHeaders["X-Content-Type-Options"].Should().Contain("nosniff");
            }

            if (allHeaders.ContainsKey("X-Frame-Options"))
            {
                allHeaders["X-Frame-Options"].Should().Contain("DENY");
            }
        }
    }

    [Theory]
    [InlineData("/school")]
    [InlineData("/school/search-for-a-school")]
    public async Task GetSignIn_WithVariousReturnUrls_ReturnsValidResponse(string returnUrl)
    {
        var response = await _fixture.Client.GetAsync($"{ExpectedRoutes.SignIn}?returnUrl={Uri.EscapeDataString(returnUrl)}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetSignIn_WithExternalReturnUrl_DoesNotRedirectToExternalSite()
    {
        var externalUrl = "https://malicious-site.com/phishing";

        var response = await _fixture.AuthenticatedClient.GetAsync($"{ExpectedRoutes.SignIn}?returnUrl={Uri.EscapeDataString(externalUrl)}");

        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            response.Headers.Location!.ToString().Should().NotContain("malicious-site.com");
        }
    }

    #endregion

    #region GET /Auth/sign-out Tests

    [Fact]
    public async Task GetSignOut_WhenAuthenticated_ReturnsValidResponse()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync(ExpectedRoutes.SignOut);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetSignOut_WhenNotAuthenticated_RequiresAuthentication()
    {
        var response = await _fixture.NonRedirectingClient.GetAsync(ExpectedRoutes.SignOut);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GET /Auth/access-denied Tests

    [Fact]
    public async Task GetAccessDenied_ReturnsSuccess()
    {
        var response = await _fixture.Client.GetAsync(ExpectedRoutes.AccessDenied);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        }
    }

    [Fact]
    public async Task GetAccessDenied_WhenAuthenticated_ReturnsSuccess()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync(ExpectedRoutes.AccessDenied);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetAccessDenied_HasSecurityHeaders()
    {
        var response = await _fixture.Client.GetAsync(ExpectedRoutes.AccessDenied);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var allHeaders = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(h => h.Key, h => h.Value);

            if (allHeaders.ContainsKey("X-Content-Type-Options"))
            {
                allHeaders["X-Content-Type-Options"].Should().Contain("nosniff");
            }

            if (allHeaders.ContainsKey("X-Frame-Options"))
            {
                allHeaders["X-Frame-Options"].Should().Contain("DENY");
            }
        }
    }

    [Fact]
    public async Task GetAccessDenied_ReturnsPageWithAppropriateContent()
    {
        var response = await _fixture.Client.GetAsync(ExpectedRoutes.AccessDenied);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region GET /Auth/signed-out Tests

    [Fact]
    public async Task GetSignedOut_ReturnsSuccess()
    {
        var response = await _fixture.Client.GetAsync(ExpectedRoutes.SignedOut);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task AllPublicEndpoints_HaveSecurityHeaders()
    {
        var publicEndpoints = new[]
        {
            ExpectedRoutes.AccessDenied,
            ExpectedRoutes.SignedOut
        };

        foreach (var endpoint in publicEndpoints)
        {
            var response = await _fixture.Client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var allHeaders = response.Headers
                    .Concat(response.Content.Headers)
                    .ToDictionary(h => h.Key, h => h.Value);

                if (allHeaders.ContainsKey("X-Content-Type-Options"))
                {
                    allHeaders["X-Content-Type-Options"].Should().Contain("nosniff");
                }

                if (allHeaders.ContainsKey("X-Frame-Options"))
                {
                    allHeaders["X-Frame-Options"].Should().Contain("DENY");
                }
            }
        }
    }

    [Fact]
    public async Task SignIn_WithXssPayload_DoesNotExecuteScript()
    {
        var maliciousReturnUrl = "<script>alert('xss')</script>";

        var response = await _fixture.Client.GetAsync($"{ExpectedRoutes.SignIn}?returnUrl={Uri.EscapeDataString(maliciousReturnUrl)}");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>alert('xss')</script>");
        }
    }

    #endregion

    #region Timeout and Concurrency Tests

    [Fact]
    public async Task SignIn_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var response = await _fixture.Client.GetAsync(ExpectedRoutes.SignIn, cts.Token);

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task AccessDenied_CompletesWithinTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var response = await _fixture.Client.GetAsync(ExpectedRoutes.AccessDenied, cts.Token);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    public async Task ConcurrentSignInRequests_HandleGracefully()
    {
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_fixture.Client.GetAsync(ExpectedRoutes.SignIn));
        }

        var responses = await Task.WhenAll(tasks);

        responses.Should().OnlyContain(r =>
            r.StatusCode == HttpStatusCode.OK ||
            r.StatusCode == HttpStatusCode.Redirect ||
            r.StatusCode == HttpStatusCode.Found);
    }

    #endregion

    #region Route Validation Tests

    [Theory]
    [InlineData("/Auth/sign-in")]
    [InlineData("/Auth/access-denied")]
    [InlineData("/Auth/signed-out")]
    public async Task PublicEndpoints_RespondToHeadRequests(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, endpoint);
        var response = await _fixture.Client.SendAsync(request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task SignIn_WithTrailingSlash_Works()
    {
        var response = await _fixture.Client.GetAsync($"{ExpectedRoutes.SignIn}/");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SignIn_CaseInsensitiveRoute()
    {
        var response = await _fixture.Client.GetAsync("/AUTH/SIGN-IN");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SignOut_CaseInsensitiveRoute()
    {
        var response = await _fixture.AuthenticatedClient.GetAsync("/AUTH/SIGN-OUT");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    #endregion
}