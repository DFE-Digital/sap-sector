using SAPSec.Core;

namespace SAPSec.Web.Middleware;

public sealed class NotFoundExceptionMiddleware(RequestDelegate next, ILogger<NotFoundExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            logger.LogError(ex, ex.Message);

            if (context.Response.HasStarted)
            {
                throw;
            }

            var originalPath = context.Request.Path;
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Request.Path = "/error/404";

            await next(context);

            context.Request.Path = originalPath;
        }
    }
}
