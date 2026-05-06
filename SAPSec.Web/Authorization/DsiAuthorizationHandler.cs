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

        void Fail(string message)
        {
            logger.LogWarning(message);
            context.Fail(new AuthorizationFailureReason(this, message));
        }

        var user = await userService.GetUserFromClaimsAsync(context.User);
        if (user is null)
        {
            Fail("User claim was null.");
            return;
        }

        var org = await userService.GetCurrentOrganisationAsync(context.User);
        if (org is null)
        {
            Fail("User Organisation claim was null.");
            return;
        }

        if (!org.IsEstablishment || org.Urn is null)
        {
            logger.LogInformation("User Organisation is not an Establishment or has a null Urn.");
        }

        context.Succeed(requirement);
    }
}