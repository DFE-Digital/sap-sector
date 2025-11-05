using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Middleware;

public class DsiOrganisationMiddleware(
    RequestDelegate next,
    ILogger<DsiOrganisationMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<DsiOrganisationMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task InvokeAsync(HttpContext context, IDsiUserService userService)
    {
        // Skip middleware for certain paths
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        if (path.StartsWith("/auth") ||
            path.StartsWith("/organisation/switch") ||
            path.StartsWith("/organisation/select") ||
            path.StartsWith("/health") ||
            path.StartsWith("/static") ||
            !context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        try
        {
            var user = await userService.GetUserFromClaimsAsync(context.User);

            if (user == null)
            {
                _logger.LogWarning("User authenticated but could not load user data");
                context.Response.Redirect("/auth/sign-in");
                return;
            }

            // Check if user has organisations
            if (!user.Organisations.Any())
            {
                _logger.LogWarning(
                    "User {UserId} has no organisations",
                    userService.GetUserId(context.User));
                context.Response.Redirect("/auth/access-denied");
                return;
            }

            // Check if user has selected an organisation
            var currentOrg = await userService.GetCurrentOrganisationAsync(context.User);
            if (currentOrg == null && user.Organisations.Count > 1)
            {
                _logger.LogInformation(
                    "User {UserId} needs to select organisation",
                    userService.GetUserId(context.User));
                context.Response.Redirect(
                    $"/auth/select-organisation?returnUrl={context.Request.Path}");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DSI organisation middleware");
            context.Response.Redirect("/Home/Error");
            return;
        }

        await _next(context);
    }
}