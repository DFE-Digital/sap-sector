namespace SAPSec.Web.Middleware;

public static class MiddlewareExtensions
{
    public static void AddMiddleware(this IApplicationBuilder builder, bool isDevelopment)
    {
        builder.UseMiddleware<SecurityHeadersMiddleware>();

        if (!isDevelopment)
            builder.UseMiddleware<DsiOrganisationMiddleware>()
                .UseMiddleware<RequireAuthenticatedUserMiddleware>();
    }
}