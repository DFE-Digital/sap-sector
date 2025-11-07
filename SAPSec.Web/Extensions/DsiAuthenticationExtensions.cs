using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SAPSec.Core.Configuration;

namespace SAPSec.Web.Extensions;

public static class DsiAuthenticationExtensions
{
    public static void AddDsiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure DSI settings
        services.AddOptions();

        services.Configure<DsiConfiguration>(configuration.GetSection("DsiConfiguration").Bind);

        services.Configure<DfeSignInSettings>(configuration.GetSection("DfeSignInSettings").Bind);

        var dsiConfig = configuration
            .GetSection("DsiConfiguration")
            .Get<DsiConfiguration>();

        if (dsiConfig == null)
        {
            throw new InvalidOperationException(
                "DsiConfiguration section not found in configuration");
        }

        // Register services
        services.AddHttpClient<IDsiApiService, DsiApiService>();
        services.AddScoped<IDsiUserService, DsiUserService>();
        services.AddHttpContextAccessor();

        // Configure session
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(60);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        // Clear default claim mappings
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // Configure authentication
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;
            options.LoginPath = "/auth/sign-in";
            options.LogoutPath = "/auth/sign-out";
            options.AccessDeniedPath = "/auth/access-denied";
            options.Cookie.Name = "SapSector.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = dsiConfig.Authority;
            options.RequireHttpsMetadata = dsiConfig.RequireHttpsMetadata;
            options.ClientId = dsiConfig.ClientId;
            options.ClientSecret = dsiConfig.ClientSecret;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.CallbackPath = dsiConfig.CallbackPath;
            options.SignedOutCallbackPath = dsiConfig.SignedOutCallbackPath;
            options.MetadataAddress = dsiConfig.MetadataAddress;

            // Configure scopes
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("organisation");

            // Configure token validation
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = dsiConfig.ValidateIssuer,
                ValidIssuer = dsiConfig.Issuer,
                ValidateAudience = dsiConfig.ValidateAudience,
                ValidAudience = dsiConfig.ClientId,
                ValidateLifetime = dsiConfig.ValidateLifetime,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Handle events
            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = OnTokenValidated,
                OnAuthenticationFailed = OnAuthenticationFailed,
                OnRemoteFailure = OnRemoteFailure,
                OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
                OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireOrganisation", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("organisation"))
            .AddPolicy("AdminOnly", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole("Admin"));
    }

    private static Task OnTokenValidated(TokenValidatedContext context)
    {
        if (context.Principal?.Identity is ClaimsIdentity identity)
        {
            // Extract and add custom claims from the token
            if (context.SecurityToken is { } token)
            {
                // Add organisation claim if present
                var orgClaim = token.Claims.FirstOrDefault(c => c.Type == "organisation");
                if (orgClaim != null && !string.IsNullOrEmpty(orgClaim.Value))
                {
                    identity.AddClaim(new Claim("organisation", orgClaim.Value));
                }

                // Add role claims if present
                var roleClaims = token.Claims.Where(c => c.Type == "role");
                foreach (var roleClaim in roleClaims)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                }
            }
        }

        return Task.CompletedTask;
    }

    private static Task OnAuthenticationFailed(AuthenticationFailedContext context)
    {
        context.Response.Redirect("/auth/error");
        context.HandleResponse();
        return Task.CompletedTask;
    }

    private static Task OnRemoteFailure(RemoteFailureContext context)
    {
        context.Response.Redirect("/auth/error");
        context.HandleResponse();
        return Task.CompletedTask;
    }

    private static Task OnRedirectToIdentityProvider(RedirectContext context)
    {
            if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
            {
                var request = context.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

                context.ProtocolMessage.PostLogoutRedirectUri = $"{baseUrl}/auth/signed-out";

                var idToken = context.HttpContext.User.FindFirst("id_token")?.Value;
                if (!string.IsNullOrEmpty(idToken))
                {
                    context.ProtocolMessage.IdTokenHint = idToken;
                }
            }
            return Task.CompletedTask;
    }

    private static Task OnRedirectToIdentityProviderForSignOut(
        RedirectContext context)
    {
        var request = context.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        var postLogoutRedirectUri = $"{baseUrl}/signout-callback-oidc";
        context.ProtocolMessage.PostLogoutRedirectUri = postLogoutRedirectUri;

        return Task.CompletedTask;
    }
}