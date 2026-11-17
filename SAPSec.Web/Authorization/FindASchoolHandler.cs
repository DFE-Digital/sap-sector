using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;

namespace SAPSec.Web.Authorization;

public class FindASchoolHandler(
    IUserService userService,
    IFeatureFlagService featureFlagService,
    IDfeUserIdentifier dfeUserIdentifier)
    : AuthorizationHandler<FindASchoolRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        FindASchoolRequirement requirement)
    {
        if (!userService.IsAuthenticated(context.User))
        {
            return;
        }

        if (await featureFlagService.IsEnabledAsync(Flags.EnableFindASchool)
            || await dfeUserIdentifier.IsDfeUserAsync(context.User))
        {
            context.Succeed(requirement);
        }
    }
}
