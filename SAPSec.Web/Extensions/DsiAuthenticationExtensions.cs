using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SAPSec.Core.Configuration;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Interfaces.Services.IDsiApiService;
using SAPSec.Core.Model.DsiConfiguration;
using SAPSec.Core.Services;
using SAPSec.Core.Services.DsiApiService;

namespace SAPSec.Web.Extensions;

public static class DsiAuthenticationExtensions
{
    public static IServiceCollection AddDsiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DsiConfiguration>(configuration.GetSection("DsiConfiguration"));

        var dsiConfig = configuration.GetSection("DsiConfiguration").Get<DsiConfiguration>();

        if (string.IsNullOrEmpty(dsiConfig?.ClientId))
        {
            throw new InvalidOperationException("DsiConfiguration:ClientId is required");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "SAPSec.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(dsiConfig.TokenExpiryMinutes);
            options.SlidingExpiration = true;
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = dsiConfig.Authority;
            options.ClientId = dsiConfig.ClientId;
            options.ClientSecret = dsiConfig.ClientSecret;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.CallbackPath = new PathString(dsiConfig.CallbackPath);
            options.SignedOutCallbackPath = new PathString(dsiConfig.SignedOutCallbackPath);
            options.RequireHttpsMetadata = dsiConfig.RequireHttpsMetadata;
            options.MetadataAddress = dsiConfig.MetadataAddress;

            // CRITICAL: Handle redirects properly for production
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    // Force HTTPS for production environment
                    if (!context.HttpContext.Request.Host.Host.Contains("localhost"))
                    {
                        var callbackPath = dsiConfig.CallbackPath;
                        var host = context.HttpContext.Request.Host.ToUriComponent();
                        context.ProtocolMessage.RedirectUri = $"https://{host}{callbackPath}";

                        // Log for debugging
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<OpenIdConnectHandler>>();
                        logger.LogInformation("DSI Redirect URI: {RedirectUri}",
                            context.ProtocolMessage.RedirectUri);
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<OpenIdConnectHandler>>();
                    logger.LogError(context.Exception, "DSI Authentication failed");
                    return Task.CompletedTask;
                }
            };

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("email");
            options.Scope.Add("profile");
            options.Scope.Add("organisation");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = dsiConfig.ValidateIssuer,
                ValidIssuer = dsiConfig.Issuer,
                ValidateAudience = dsiConfig.ValidateAudience,
                ValidAudience = dsiConfig.Audience,
                ValidateLifetime = dsiConfig.ValidateLifetime,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IDsiUserService, DsiUserService>();
        services.AddHttpClient<IDsiApiService, DsiApiService>();

        return services;
    }
}