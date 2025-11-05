using SAPSec.Web.Middleware;

namespace SAPSec.Web.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseDsiOrganisationMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DsiOrganisationMiddleware>();
    }
}