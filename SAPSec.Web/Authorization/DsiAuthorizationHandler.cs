using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;

namespace SAPSec.Web.Authorization;

public class DsiAuthorizationHandler(
    IUserService userService,
    IInternalStaffChecker internalStaffChecker,
    ILogger<DsiAuthorizationHandler> logger)
    : AuthorizationHandler<DsiAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DsiAuthorizationRequirement requirement)
    {
        if (!userService.IsAuthenticated(context.User))
        {
            return;
        }

        var user = await userService.GetUserFromClaimsAsync(context.User);
        if (user is null)
        {
            var userName = context.User.Identity?.Name ?? "(unknown)";
            logger.LogWarning("User claim was null for user {UserName}.", userName);
            context.Fail(new AuthorizationFailureReason(this, $"User claim was null for user {userName}."));
            return;
        }

        var userId = user.Sub;

        var org = await userService.GetCurrentOrganisationAsync(context.User);
        if (org is null)
        {
            logger.LogWarning("User Organisation claim was null for user {UserId}.", userId);
            context.Fail(new AuthorizationFailureReason(this, $"User Organisation claim was null for user {userId}."));
            return;
        }

        var requestedUrn = (context.Resource as HttpContext)?.GetRouteValue("urn")?.ToString();

        if (!string.IsNullOrWhiteSpace(requestedUrn))
        {
            if (internalStaffChecker.IsInternalStaff(org))
            {
                logger.LogInformation(
                    "Internal staff access to URN {RequestedUrn} for user {UserId}. OrganisationId: {OrganisationId}.",
                    requestedUrn,
                    userId,
                    org.Id);
                context.Succeed(requirement);
                return;
            }

            var allowed = org.IsEstablishment
                && string.Equals(org.Urn?.Trim(), requestedUrn.Trim(), StringComparison.Ordinal);

            if (!allowed)
            {
                logger.LogWarning(
                    "Access denied to URN {RequestedUrn} for user {UserId}. OrganisationId: {OrganisationId}. Category: {OrganisationCategory}. Urn: {OrganisationUrn}.",
                    requestedUrn,
                    userId,
                    org.Id,
                    org.Category?.Name,
                    org.Urn);
                context.Fail(new AuthorizationFailureReason(this, $"User {userId} is not permitted to access URN {requestedUrn}."));
                return;
            }
        }
        else if (!org.IsEstablishment || org.Urn is null)
        {
            logger.LogInformation(
                "User Organisation is not an Establishment or has a null Urn for user {UserId}. OrganisationId: {OrganisationId}. Category: {OrganisationCategory}. Urn: {OrganisationUrn}.",
                userId,
                org.Id,
                org.Category?.Name,
                org.Urn);
        }

        context.Succeed(requirement);
    }
}
