using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;

namespace SAPSec.Web.Authorization;

public class BreadcrumbNavigationHandler(
    IUserService userService,
    IFeatureFlagService featureFlagService)
    : AuthorizationHandler<BreadcrumbNavigationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BreadcrumbNavigationRequirement requirement)
    {
        if (await featureFlagService.IsEnabledAsync(Flags.EnableFindASchool)
            || !userService.IsAuthenticated(context.User))
        {
            context.Succeed(requirement);
            return;
        }

        var org = await userService.GetCurrentOrganisationAsync(context.User);

        // Only school users lose the breadcrumbs so far; other organisation types are handled by their own stories.
        if (org is not { IsEstablishment: true })
        {
            context.Succeed(requirement);
        }
    }
}
