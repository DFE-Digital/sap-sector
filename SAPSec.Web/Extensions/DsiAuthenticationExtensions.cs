using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SAPSec.Core.Configuration;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DsiAuthenticationExtensions
{
    private static class Routes
    {
        public const string SignIn = "/Auth/sign-in";
        public const string AccessDenied = "/Auth/access-denied";
        public const string Home = "/";
        public const string SchoolSearch = "/search-for-a-school";
        public const string Error = "/error";
    }

    private static class CookieSettings
    {
        public const string Name = "SAPSec.Auth";
    }

    private static class ClaimTypes
    {
        public const string Subject = "sub";
        public const string Email = "email";
        public const string Name = "name";
        public const string Organisation = "organisation";
        public const string Role = "role";
    }

    public static IServiceCollection AddDsiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dsiConfig = GetAndValidateConfiguration(configuration);

        services.Configure<DsiConfiguration>(configuration.GetSection("DsiConfiguration"));

        services
            .AddAuthentication(ConfigureAuthenticationDefaults)
            .AddCookie(options => ConfigureCookieOptions(options, dsiConfig))
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,
                options => ConfigureOpenIdConnectOptions(options, dsiConfig));

        RegisterServices(services);

        return services;
    }

    private static DsiConfiguration GetAndValidateConfiguration(IConfiguration configuration)
    {
        var dsiConfig = configuration.GetSection("DsiConfiguration").Get<DsiConfiguration>();

        if (string.IsNullOrEmpty(dsiConfig?.ClientId))
        {
            throw new InvalidOperationException("DsiConfiguration:ClientId is required");
        }

        return dsiConfig;
    }

    private static void ConfigureAuthenticationDefaults(AuthenticationOptions options)
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    }

    private static void ConfigureCookieOptions(CookieAuthenticationOptions options, DsiConfiguration config)
    {
        options.Cookie.Name = CookieSettings.Name;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(config.TokenExpiryMinutes);
        options.SlidingExpiration = true;
        options.LoginPath = Routes.SignIn;
        options.AccessDeniedPath = Routes.AccessDenied;
    }

    private static void ConfigureOpenIdConnectOptions(OpenIdConnectOptions options, DsiConfiguration config)
    {
        ConfigureBasicOpenIdConnectSettings(options, config);
        ConfigureOpenIdConnectScopes(options);
        ConfigureTokenValidation(options, config);
        options.Events = CreateOpenIdConnectEvents(config);
    }

    private static void ConfigureBasicOpenIdConnectSettings(OpenIdConnectOptions options, DsiConfiguration config)
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = config.Authority;
        options.ClientId = config.ClientId;
        options.ClientSecret = config.ClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.CallbackPath = new PathString(config.CallbackPath);
        options.SignedOutCallbackPath = new PathString(config.SignedOutCallbackPath);
        options.RequireHttpsMetadata = config.RequireHttpsMetadata;
        options.MetadataAddress = config.MetadataAddress;
    }

    private static void ConfigureOpenIdConnectScopes(OpenIdConnectOptions options)
    {
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.Scope.Add("organisation");
    }

    private static void ConfigureTokenValidation(OpenIdConnectOptions options, DsiConfiguration config)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = config.ValidateIssuer,
            ValidIssuer = config.Issuer,
            ValidateAudience = config.ValidateAudience,
            ValidAudience = config.Audience,
            ValidateLifetime = config.ValidateLifetime,
            ClockSkew = TimeSpan.FromMinutes(5),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    }

    private static OpenIdConnectEvents CreateOpenIdConnectEvents(DsiConfiguration config)
    {
        return new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context => HandleRedirectToIdentityProvider(context, config),
            OnMessageReceived = HandleMessageReceived,
            OnAuthorizationCodeReceived = HandleAuthorizationCodeReceived,
            OnTokenValidated = HandleTokenValidated,
            OnRemoteFailure = HandleRemoteFailure,
            OnAuthenticationFailed = HandleAuthenticationFailed,
            OnSignedOutCallbackRedirect = HandleSignedOutCallbackRedirect
        };
    }

    private static Task HandleRedirectToIdentityProvider(
        RedirectContext context,
        DsiConfiguration config)
    {
        if (IsNonLocalhost(context))
        {
            SetProductionRedirectUri(context, config);
        }

        return Task.CompletedTask;
    }

    private static bool IsNonLocalhost(RedirectContext context)
    {
        return !context.HttpContext.Request.Host.Host.Contains("localhost");
    }

    private static void SetProductionRedirectUri(RedirectContext context, DsiConfiguration config)
    {
        var host = context.HttpContext.Request.Host.ToUriComponent();
        context.ProtocolMessage.RedirectUri = $"https://{host}{config.CallbackPath}";
    }

    private static Task HandleMessageReceived(MessageReceivedContext context)
    {
        if (IsSpuriousAuthCallbackRequest(context))
        {
            LogSpuriousRequest(context);
            RedirectToHomeAndHandle(context);
        }

        return Task.CompletedTask;
    }

    private static bool IsSpuriousAuthCallbackRequest(MessageReceivedContext context)
    {
        var options = context.Options;
        return context.Request.Path == options.CallbackPath &&
               context.Request.Method == "GET" &&
               !context.Request.Query.ContainsKey("code");
    }

    private static void LogSpuriousRequest(MessageReceivedContext context)
    {
        var logger = GetLogger(context.HttpContext);
        logger.LogWarning(
            "Spurious authentication callback request detected at {Path}",
            context.Request.Path);
    }

    private static void RedirectToHomeAndHandle(MessageReceivedContext context)
    {
        context.Response.Redirect(Routes.Home);
        context.HandleResponse();
    }

    private static Task HandleAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        return Task.CompletedTask;
    }

    private static async Task HandleTokenValidated(TokenValidatedContext context)
    {
        var logger = GetLogger(context.HttpContext);

        try
        {
            await ProcessValidatedToken(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing validated token for user");
            context.Fail(ex);
        }
    }

    private static async Task ProcessValidatedToken(TokenValidatedContext context)
    {
        var userService = context.HttpContext.RequestServices
            .GetRequiredService<IUserService>();

        var user = await userService.GetUserFromClaimsAsync(context.Principal!);

        if (user == null)
        {
            return;
        }

        await SetOrganisationBasedOnCount(context, userService, user);
    }

    private static async Task SetOrganisationBasedOnCount(
        TokenValidatedContext context,
        IUserService userService,
        User user)
    {
        if (user.Organisations.Count == 1)
        {
            await userService.SetCurrentOrganisationAsync(
                context.Principal!,
                user.Organisations[0].Id);
        }
        else if (user.Organisations.Count > 1)
        {
            context.Properties!.RedirectUri = Routes.SchoolSearch;
        }
    }

    private static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        var logger = GetLogger(context.HttpContext);

        logger.LogError(
            context.Failure,
            "DSI remote authentication failure: {Message}",
            context.Failure?.Message);

        RedirectToErrorAndHandle(context);

        return Task.CompletedTask;
    }

    private static Task HandleAuthenticationFailed(AuthenticationFailedContext context)
    {
        var logger = GetLogger(context.HttpContext);

        logger.LogError(
            context.Exception,
            "DSI authentication failed: {Message}",
            context.Exception.Message);

        RedirectToErrorAndHandle(context);

        return Task.CompletedTask;
    }

    private static void RedirectToErrorAndHandle(RemoteFailureContext context)
    {
        context.Response.Redirect(Routes.Error);
        context.HandleResponse();
    }

    private static void RedirectToErrorAndHandle(AuthenticationFailedContext context)
    {
        context.Response.Redirect(Routes.Error);
        context.HandleResponse();
    }

    private static Task HandleSignedOutCallbackRedirect(RemoteSignOutContext context)
    {
        context.Response.Redirect(Routes.Home);
        context.HandleResponse();

        return Task.CompletedTask;
    }

    private static ILogger<Program> GetLogger(HttpContext httpContext)
    {
        return httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserService, UserService>();
        RegisterDsiApiClient(services);
    }

    private static void RegisterDsiApiClient(IServiceCollection services)
    {
        services.AddHttpClient<IDsiClient, DsiApiService>(ConfigureDsiApiClient);
    }

    private static void ConfigureDsiApiClient(IServiceProvider serviceProvider, HttpClient client)
    {
        var config = GetDsiConfigurationForApiClient(serviceProvider);

        client.BaseAddress = new Uri(config.ApiUri);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);
    }

    private static DsiConfiguration GetDsiConfigurationForApiClient(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var dsiConfig = configuration.GetSection("DsiConfiguration").Get<DsiConfiguration>();

        if (string.IsNullOrEmpty(dsiConfig?.ApiUri))
        {
            throw new InvalidOperationException("DsiConfiguration:ApiBaseUrl is required");
        }

        return dsiConfig;
    }
}