namespace SAPSec.Web.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder AddMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>()
            .UseMiddleware<DsiOrganisationMiddleware>()
            .UseMiddleware<RequireAuthenticatedUserMiddleware>();
    }
}