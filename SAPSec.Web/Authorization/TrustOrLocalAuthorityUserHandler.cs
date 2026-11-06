using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;

namespace SAPSec.Web.Authorization;

public class TrustOrLocalAuthorityUserHandler(
    IUserService userService,
    ILogger<TrustOrLocalAuthorityUserHandler> logger)
    : AuthorizationHandler<TrustOrLocalAuthorityUserRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TrustOrLocalAuthorityUserRequirement requirement)
    {
        if (!userService.IsAuthenticated(context.User))
        {
            return;
        }

        var org = await userService.GetCurrentOrganisationAsync(context.User);
        if (org is null)
        {
            logger.LogWarning(
                "Current organisation was null for user {UserId} when evaluating trust/local authority policy.",
                userService.GetUserId(context.User));
            return;
        }

        if (org.IsTrust || org.IsLocalAuthority)
        {
            context.Succeed(requirement);
            return;
        }

        logger.LogInformation(
            "User {UserId} does not meet trust/local authority policy. OrganisationId: {OrganisationId}. CategoryId: {CategoryId}. Category: {OrganisationCategory}.",
            userService.GetUserId(context.User),
            org.Id,
            org.Category?.Id,
            org.Category?.Name);
    }
}
