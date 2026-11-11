using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;

namespace SAPSec.Web.Authorization;

public class BreadcrumbNavigationHandler(
    IUserService userService,
    IFeatureFlagService featureFlagService,
    IDfeUserIdentifier dfeUserIdentifier)
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

        // The "Home" breadcrumb leads to "Find a school", which only DfE staff can use.
        if (await dfeUserIdentifier.IsDfeUserAsync(context.User))
        {
            context.Succeed(requirement);
        }
    }
}
