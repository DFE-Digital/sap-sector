using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;

namespace SAPSec.Web.Authorization;

public class DfeUserHandler(IUserService userService) : AuthorizationHandler<DfeUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DfeUserRequirement requirement)
    {
        if (userService.IsAuthenticated(context.User)
            && userService.HasRole(context.User, UserRoles.DfeUser))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
