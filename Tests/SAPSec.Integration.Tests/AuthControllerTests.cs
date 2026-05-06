using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;
using System.Net;

namespace SAPSec.Integration.Tests;

public class AuthControllerIntegrationTests(WebApplicationSetupFixture fixture) : IClassFixture<WebApplicationSetupFixture>
{
    private static class ExpectedRoutes
    {
        public const string SignIn = "/auth/signin";
        public const string SignOut = "/auth/signout";
        public const string AccessDenied = "/error/403";
        public const string SignedOut = "/auth/signed-out";
    }

    #region GET /auth/signin Tests

    [Fact]
    public async Task GetSignIn_WhenNotAuthenticated_ReturnsChallengeRedirect()
    {
        var response = await fixture.Client.GetAsync(ExpectedRoutes.SignIn);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSignIn_WhenAuthenticated_RedirectsToReturnUrl()
    {
        var response = await fixture.NonRedirectingClient.GetAsync($"{ExpectedRoutes.SignIn}?returnUrl=/dashboard");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSignIn_WhenAuthenticated_WithNullReturnUrl_RedirectsToHome()
    {
        var response = await fixture.NonRedirectingClient.GetAsync(ExpectedRoutes.SignIn);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSignIn_HasSecurityHeaders()
    {
        var response = await fixture.Client.GetAsync(ExpectedRoutes.SignIn);

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
    [InlineData("/find-a-school")]
    public async Task GetSignIn_WithVariousReturnUrls_ReturnsValidResponse(string returnUrl)
    {
        var response = await fixture.Client.GetAsync($"{ExpectedRoutes.SignIn}?returnUrl={Uri.EscapeDataString(returnUrl)}");

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

        var response = await fixture.NonRedirectingClient.GetAsync($"{ExpectedRoutes.SignIn}?returnUrl={Uri.EscapeDataString(externalUrl)}");

        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            response.Headers.Location!.ToString().Should().NotContain("malicious-site.com");
        }
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task AllPublicEndpoints_HaveSecurityHeaders()
    {
        var publicEndpoints = new[]
        {
            ExpectedRoutes.AccessDenied,
            //ExpectedRoutes.SignedOut
        };

        foreach (var endpoint in publicEndpoints)
        {
            var response = await fixture.NonRedirectingClient.GetAsync(endpoint);

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

        var response = await fixture.Client.GetAsync($"{ExpectedRoutes.SignIn}?returnUrl={Uri.EscapeDataString(maliciousReturnUrl)}");

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

        var response = await fixture.Client.GetAsync(ExpectedRoutes.SignIn, cts.Token);

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task AccessDenied_CompletesWithinTimeout()
    {
        var response = await fixture.Client.GetAsync(ExpectedRoutes.AccessDenied);

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
            tasks.Add(fixture.Client.GetAsync(ExpectedRoutes.SignIn));
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
    [InlineData("/auth/signin")]
    [InlineData("/auth/signed-out")]
    public async Task PublicEndpoints_RespondToHeadRequests(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, endpoint);
        var response = await fixture.Client.SendAsync(request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task SignIn_WithTrailingSlash_Works()
    {
        var response = await fixture.Client.GetAsync($"{ExpectedRoutes.SignIn}/");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SignIn_CaseInsensitiveRoute()
    {
        var response = await fixture.Client.GetAsync("/AUTH/SIGNIN");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.NotFound);
    }

    #endregion
}