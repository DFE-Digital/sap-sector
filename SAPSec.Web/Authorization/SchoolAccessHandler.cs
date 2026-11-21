using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;

namespace SAPSec.Web.Authorization;

public class SchoolAccessHandler(
    IUserService userService,
    IFeatureFlagService featureFlagService,
    IDfeUserIdentifier dfeUserIdentifier,
    ILogger<SchoolAccessHandler> logger)
    : AuthorizationHandler<SchoolAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SchoolAccessRequirement requirement)
    {
        if (!userService.IsAuthenticated(context.User))
        {
            return;
        }

        // DfE staff can open any school they can find, even when the organisation they signed in as is a school.
        if (await featureFlagService.IsEnabledAsync(Flags.EnableFindASchool)
            || await dfeUserIdentifier.IsDfeUserAsync(context.User))
        {
            context.Succeed(requirement);
            return;
        }

        var org = await userService.GetCurrentOrganisationAsync(context.User);
        if (org is null)
        {
            logger.LogWarning(
                "Current organisation was null for user {UserId} when evaluating school access policy.",
                userService.GetUserId(context.User));
            return;
        }

        if (!org.IsEstablishment)
        {
            context.Succeed(requirement);
            return;
        }

        var requestedUrn = (context.Resource as HttpContext)?.GetRouteValue(SchoolAccessRequirement.UrnRouteValueName)?.ToString();

        if (!string.IsNullOrWhiteSpace(requestedUrn) && IsOwnSchool(org, requestedUrn))
        {
            context.Succeed(requirement);
            return;
        }

        logger.LogWarning(
            "School user {UserId} denied access. OrganisationId: {OrganisationId}. OrganisationUrn: {OrganisationUrn}. RequestedUrn: {RequestedUrn}.",
            userService.GetUserId(context.User),
            org.Id,
            org.Urn,
            string.IsNullOrWhiteSpace(requestedUrn) ? "(none)" : requestedUrn);
    }

    private static bool IsOwnSchool(Organisation org, string requestedUrn) =>
        !string.IsNullOrWhiteSpace(org.Urn)
        && string.Equals(org.Urn.Trim(), requestedUrn.Trim(), StringComparison.OrdinalIgnoreCase);
}
