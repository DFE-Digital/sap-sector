using Microsoft.AspNetCore.Diagnostics;
using SAPSec.Core;

namespace SAPSec.Web.Middleware;

public sealed class NotFoundExceptionHandler(
    ILogger<NotFoundExceptionHandler> logger,
    IHostEnvironment hostEnvironment)
    : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken cancellationToken)
    {
        logger.LogError(ex, ex.Message);

        // A Core.NotFoundException indicates the underlying data to service the request could not be found.
        // This translates directly to a status code 404 so rather than catching explicitly in every controller
        // action, we bubble up and handle by redirecting execution to the 404 error page.
        if (ex is NotFoundException)
        {

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Request.Path = "/error/404";

            // For lower environments we can surface the error message on the 404 error page to help with debugging
            if (!hostEnvironment.IsProduction())
            {
                context.Items["ErrorMessage"] = ex.Message;
            }

            // Mark as handled so it doesn't get treated as a 500 error
            return ValueTask.FromResult(true);
        }

        // All other exceptions get handled by the 500 error page/developer exception page

        // For lower environments we can surface the error message on the 500 error page to help with debugging
        if (!hostEnvironment.IsProduction())
        {
            context.Items["ErrorMessage"] = ex.Message;
        }

        return ValueTask.FromResult(false);
    }
}
