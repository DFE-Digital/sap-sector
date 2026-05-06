using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SAPSec.Core.Configuration;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Authentication;

public static class DsiAuthenticationHandler
{
    public static Task HandleRedirectToIdentityProvider(
        RedirectContext context,
        DsiConfiguration config)
    {
        if (IsNonLocalhost(context))
        {
            SetProductionRedirectUri(context, config);
        }

        return Task.CompletedTask;
    }

    public static Task HandleMessageReceived(MessageReceivedContext context)
    {
        if (IsSpuriousAuthCallbackRequest(context))
        {
            LogSpuriousRequest(context);
            RedirectToHomeAndHandle(context);
        }

        return Task.CompletedTask;
    }

    public static Task HandleAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        return Task.CompletedTask;
    }

    public static async Task HandleTokenValidated(TokenValidatedContext context)
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

    public static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        var logger = GetLogger(context.HttpContext);

        logger.LogError(
            context.Failure,
            "DSI remote authentication failure: {Message}",
            context.Failure?.Message);

        RedirectToErrorAndHandle(context);

        return Task.CompletedTask;
    }

    public static Task HandleAuthenticationFailed(AuthenticationFailedContext context)
    {
        var logger = GetLogger(context.HttpContext);

        logger.LogError(
            context.Exception,
            "DSI authentication failed: {Message}",
            context.Exception.Message);

        RedirectToErrorAndHandle(context);

        return Task.CompletedTask;
    }

    public static Task HandleSignedOutCallbackRedirect(RemoteSignOutContext context)
    {
        context.Response.Redirect(Routes.Home);
        context.HandleResponse();

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
            context.Properties!.RedirectUri = Routes.FindASchool;
        }
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

    private static ILogger<Program> GetLogger(HttpContext httpContext)
    {
        return httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    }
}
