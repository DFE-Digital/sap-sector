using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SAPSec.Core.Configuration;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services;

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

                // Event handlers for authentication lifecycle
                options.Events = new OpenIdConnectEvents
                {
                    // Triggered when redirecting to DSI for authentication
                    OnRedirectToIdentityProvider = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        logger.LogInformation("User initiating sign-in to DSI");

                        // Force HTTPS redirect_uri for non-localhost environments
                        if (!context.HttpContext.Request.Host.Host.Contains("localhost"))
                        {
                            var callbackPath = dsiConfig.CallbackPath;
                            var host = context.HttpContext.Request.Host.ToUriComponent();
                            context.ProtocolMessage.RedirectUri = $"https://{host}{callbackPath}";

                            logger.LogInformation("DSI Redirect URI: {RedirectUri}",
                                context.ProtocolMessage.RedirectUri);
                        }

                        // Debug logging (can be removed after testing)
                        logger.LogDebug("Client ID: {ClientId}", dsiConfig.ClientId);
                        logger.LogDebug("Client Secret configured: {HasSecret}",
                            !string.IsNullOrEmpty(dsiConfig.ClientSecret));
                        logger.LogDebug("Client Secret length: {Length}",
                            dsiConfig.ClientSecret?.Length ?? 0);

                        return Task.CompletedTask;
                    },

                    // Triggered when receiving message from DSI (before processing)
                    // Handles spurious callback requests
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        // Check for spurious authentication callback requests
                        // (callbacks without an authorization code)
                        var isSpuriousAuthCbRequest =
                            context.Request.Path == options.CallbackPath &&
                            context.Request.Method == "GET" &&
                            !context.Request.Query.ContainsKey("code");

                        if (isSpuriousAuthCbRequest)
                        {
                            logger.LogWarning(
                                "Spurious authentication callback request detected at {Path}",
                                context.Request.Path);

                            context.Response.Redirect("/");
                            context.HandleResponse();
                        }

                        return Task.CompletedTask;
                    },

                    // Triggered when authorization code is received from DSI
                    // Before token exchange
                    OnAuthorizationCodeReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        logger.LogInformation("Authorization code received from DSI, exchanging for tokens");

                        // Debug logging for token exchange
                        logger.LogDebug("Token endpoint: {Endpoint}",
                            context.Options.Configuration?.TokenEndpoint);
                        logger.LogDebug("Code received: {HasCode}",
                            !string.IsNullOrEmpty(context.TokenEndpointRequest?.Code));

                        return Task.CompletedTask;
                    },

                    OnTokenValidated = async context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        try
                        {
                            // Extract claims
                            var userId = context.Principal?.FindFirst("sub")?.Value;
                            var email = context.Principal?.FindFirst("email")?.Value;
                            var name = context.Principal?.FindFirst("name")?.Value;
                            var organisation = context.Principal?.FindFirst("organisation")?.Value;

                            logger.LogInformation(
                                "User authenticated successfully. " +
                                "UserId: {UserId}, Email: {Email}, Name: {Name}, Organisation: {Organisation}",
                                userId, email, name, organisation);

                            // ✅ Handle organization selection HERE
                            var userService = context.HttpContext.RequestServices
                                .GetRequiredService<IDsiUserService>();

                            var user = await userService.GetUserFromClaimsAsync(context.Principal!);

                            if (user != null)
                            {
                                // If user has one organisation, set it automatically
                                if (user.Organisations.Count == 1)
                                {
                                    await userService.SetCurrentOrganisationAsync(
                                        context.Principal!,
                                        user.Organisations[0].Id);

                                    logger.LogInformation(
                                        "Automatically set organisation {OrgId} for user {UserId}",
                                        user.Organisations[0].Id, userId);
                                }
                                // If multiple organisations, redirect to selection page
                                else if (user.Organisations.Count > 1)
                                {
                                    logger.LogInformation(
                                        "User {UserId} has {Count} organisations, needs to select one",
                                        userId, user.Organisations.Count);

                                    // Store original return URL
                                    var returnUrl = context.Properties?.Items["returnUrl"] ?? "/Search";
                                    context.Properties!.RedirectUri = $"/Auth/select-organisation?returnUrl={returnUrl}";
                                }
                            }

                            return;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error processing validated token for user");
                            context.Fail(ex);
                            return;
                        }
                    },

                    // Triggered when remote authentication fails
                    // This catches errors from DSI (like invalid_client)
                    OnRemoteFailure = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        logger.LogError(context.Failure,
                            "DSI remote authentication failure: {Message}",
                            context.Failure?.Message);

                        // Redirect to error page with message
                        var errorMessage = context.Failure?.Message ?? "Authentication failed";
                        context.Response.Redirect(
                            $"/Home/Error?message={Uri.EscapeDataString(errorMessage)}");
                        context.HandleResponse();

                        return Task.CompletedTask;
                    },

                    // Triggered when authentication fails for other reasons
                    // (correlation, state, etc.)
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        logger.LogError(context.Exception,
                            "DSI authentication failed: {Message}",
                            context.Exception.Message);

                        // Show detailed error in development mode
                        var env = context.HttpContext.RequestServices
                            .GetRequiredService<IWebHostEnvironment>();

                        if (env.IsDevelopment())
                        {
                            var errorMessage = context.Exception.Message;
                            context.Response.Redirect(
                                $"/Home/Error?message={Uri.EscapeDataString(errorMessage)}");
                            context.HandleResponse();
                        }
                        else
                        {
                            // In production, show generic error
                            context.Response.Redirect("/Home/Error");
                            context.HandleResponse();
                        }

                        return Task.CompletedTask;
                    },

                    // Triggered when user signs out
                    OnSignedOutCallbackRedirect = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        logger.LogInformation("User signed out from DSI");

                        // Redirect to home page after sign out
                        context.Response.Redirect("/");
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }
                };

                // Configure required scopes
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Scope.Add("organisation");

                // Token validation parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = dsiConfig.ValidateIssuer,
                    ValidIssuer = dsiConfig.Issuer,
                    ValidateAudience = dsiConfig.ValidateAudience,
                    ValidAudience = dsiConfig.Audience,
                    ValidateLifetime = dsiConfig.ValidateLifetime,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        // Register DSI-related services
        services.AddHttpContextAccessor();
        services.AddScoped<IDsiUserService, DsiUserService>();
        services.AddHttpClient<IDsiApiService, DsiApiService>();

        return services;
    }
}