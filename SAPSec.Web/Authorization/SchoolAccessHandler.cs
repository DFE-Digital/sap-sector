using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Web.Services;

namespace SAPSec.Web.Authorization;

public class SchoolAccessHandler(
    IUserService userService,
    IRequestSchoolAccessor requestSchoolAccessor,
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

        var org = await userService.GetCurrentOrganisationAsync(context.User);
        if (org is null)
        {
            logger.LogWarning(
                "Current organisation was null for user {UserId} when evaluating school access policy.",
                userService.GetUserId(context.User));
            return;
        }

        // Only trust and local authority users are restricted to their associated schools.
        if (!org.IsTrust && !org.IsLocalAuthority)
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not HttpContext httpContext
            || httpContext.GetRouteValue(SchoolAccessRequirement.UrnRouteValueName)?.ToString() is not { Length: > 0 } urn)
        {
            logger.LogWarning(
                "No school URN route value available when evaluating school access policy for user {UserId}.",
                userService.GetUserId(context.User));
            return;
        }

        var school = await requestSchoolAccessor.GetAsync(httpContext, urn);

        if (IsAssociated(org, school))
        {
            context.Succeed(requirement);
            return;
        }

        logger.LogWarning(
            "User {UserId} denied access to school {Urn}. OrganisationId: {OrganisationId}. CategoryId: {CategoryId}.",
            userService.GetUserId(context.User),
            urn,
            org.Id,
            org.Category?.Id);
    }

    private static bool IsAssociated(Organisation org, SchoolDetails school)
    {
        if (org.IsTrust)
        {
            return Matches(org.Uid, school.AcademyTrustId.Value);
        }

        if (org.IsLocalAuthority)
        {
            return Matches(org.EstablishmentNumber, school.LocalAuthorityCode.Value);
        }

        return false;
    }

    private static bool Matches(string? organisationValue, string? schoolValue) =>
        !string.IsNullOrWhiteSpace(organisationValue)
        && !string.IsNullOrWhiteSpace(schoolValue)
        && string.Equals(organisationValue.Trim(), schoolValue.Trim(), StringComparison.OrdinalIgnoreCase);
}
