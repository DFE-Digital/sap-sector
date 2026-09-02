using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Filters;

public sealed class RequireFeatureFlagFilter(
    IFeatureFlagService featureFlagService,
    string expectedFeatureFlag) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var enabled = await featureFlagService.IsEnabledAsync(expectedFeatureFlag);
        if (!enabled)
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}
