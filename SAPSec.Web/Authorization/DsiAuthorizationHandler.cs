using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Authorization;

public class DsiAuthorizationHandler(
    IUserService userService,
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

        if (!org.IsEstablishment || org.Urn is null)
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
