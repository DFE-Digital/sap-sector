using Microsoft.AspNetCore.Authorization;
using SAPSec.Web.Authorization;

namespace SAPSec.Test.Common.Authentication;

/// <summary>
/// Test double for <see cref="DsiAuthorizationHandler"/> that lets any authenticated user open any URN.
/// Existing page tests visit many schools while the mock user belongs to a single establishment, so they
/// use this handler. Tests that cover URN access itself swap the real handler back in.
/// </summary>
public class AllowAllUrnsDsiAuthorizationHandler : AuthorizationHandler<DsiAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DsiAuthorizationRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
