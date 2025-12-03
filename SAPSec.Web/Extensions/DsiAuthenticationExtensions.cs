using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                options.LoginPath = "/Auth/sign-in";
                options.AccessDeniedPath = "/Auth/access-denied";
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

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        if (!context.HttpContext.Request.Host.Host.Contains("localhost"))
                        {
                            var callbackPath = dsiConfig.CallbackPath;
                            var host = context.HttpContext.Request.Host.ToUriComponent();
                            context.ProtocolMessage.RedirectUri = $"https://{host}{callbackPath}";

                        }

                        return Task.CompletedTask;
                    },

                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

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

                    OnAuthorizationCodeReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        return Task.CompletedTask;
                    },

                    OnTokenValidated = async context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        try
                        {
                            var userId = context.Principal?.FindFirst("sub")?.Value;
                            var email = context.Principal?.FindFirst("email")?.Value;
                            var name = context.Principal?.FindFirst("name")?.Value;
                            var organisation = context.Principal?.FindFirst("organisation")?.Value;

                            var userService = context.HttpContext.RequestServices
                                .GetRequiredService<IDsiUserService>();

                            var user = await userService.GetUserFromClaimsAsync(context.Principal!);

                            if (user != null)
                            {
                                if (user.Organisations.Count == 1)
                                {
                                    await userService.SetCurrentOrganisationAsync(
                                        context.Principal!,
                                        user.Organisations[0].Id);
                                }
                                else if (user.Organisations.Count > 1)
                                {
                                    var returnUrl = context.Properties?.Items["returnUrl"] ?? "/search-for-a-school";
                                    context.Properties!.RedirectUri = $"/search-for-a-school";
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

                    OnRemoteFailure = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        logger.LogError(context.Failure,
                            "DSI remote authentication failure: {Message}",
                            context.Failure?.Message);

                        var errorMessage = context.Failure?.Message ?? "Authentication failed";
                        context.Response.Redirect(
                            $"/error");
                        context.HandleResponse();

                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        logger.LogError(context.Exception,
                            "DSI authentication failed: {Message}",
                            context.Exception.Message);

                        var env = context.HttpContext.RequestServices
                            .GetRequiredService<IWebHostEnvironment>();


                        var errorMessage = context.Exception.Message;
                        context.Response.Redirect(
                            $"/error");
                        context.HandleResponse();

                        return Task.CompletedTask;
                    },

                    OnSignedOutCallbackRedirect = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        context.Response.Redirect("/");
                        context.HandleResponse();

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
                    ClockSkew = TimeSpan.FromMinutes(5),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IDsiUserService, DsiUserService>();
        services.AddHttpClient<IDsiApiService, DsiApiService>();

        services.AddHttpClient<IDsiApiService, DsiApiService>((serviceProvider, client) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var dsiConfig = configuration.GetSection("DsiConfiguration").Get<DsiConfiguration>();

            if (string.IsNullOrEmpty(dsiConfig?.ApiUri))
            {
                throw new InvalidOperationException("DsiConfiguration:ApiBaseUrl is required");
            }

            client.BaseAddress = new Uri(dsiConfig.ApiUri);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}