using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Authentication;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Web.Services;

namespace SAPSec.Web.Authorization;

/// <summary>
/// Decides whether the current organisation may view the school identified by the "urn" route value.
/// Internal staff may view any school, establishments only their own, and trusts / local authorities
/// the schools associated with them. Everything else is denied.
/// </summary>
public class SchoolAccessHandler(
    IUserService userService,
    IInternalStaffChecker internalStaffChecker,
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

        var userId = userService.GetUserId(context.User);

        var org = await userService.GetCurrentOrganisationAsync(context.User);
        if (org is null)
        {
            logger.LogWarning(
                "Current organisation was null for user {UserId} when evaluating school access policy.",
                userId);
            return;
        }

        if (internalStaffChecker.IsInternalStaff(org))
        {
            logger.LogInformation(
                "Internal staff access for user {UserId}. OrganisationId: {OrganisationId}.",
                userId,
                org.Id);
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not HttpContext httpContext
            || httpContext.GetRouteValue(SchoolAccessRequirement.UrnRouteValueName)?.ToString() is not { Length: > 0 } urn)
        {
            logger.LogWarning(
                "No school URN route value available when evaluating school access policy for user {UserId}.",
                userId);
            return;
        }

        if (org.IsEstablishment)
        {
            if (Matches(org.Urn, urn))
            {
                context.Succeed(requirement);
                return;
            }
        }
        else if (org.IsTrust || org.IsLocalAuthority || org.IsDiocese)
        {
            var school = await requestSchoolAccessor.GetAsync(httpContext, urn);

            if (IsAssociated(org, school))
            {
                context.Succeed(requirement);
                return;
            }
        }

        logger.LogWarning(
            "User {UserId} denied access to school {Urn}. OrganisationId: {OrganisationId}. CategoryId: {CategoryId}.",
            userId,
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

        if (org.IsDiocese)
        {
            // PLACEHOLDER: which organisation field carries the diocese code is not confirmed
            // (Uid is assumed, as for trusts). Confirm against a real DSI Diocese claim.
            return Matches(org.Uid, school.DioceseId.Value);
        }

        return false;
    }

    private static bool Matches(string? organisationValue, string? schoolValue) =>
        !string.IsNullOrWhiteSpace(organisationValue)
        && !string.IsNullOrWhiteSpace(schoolValue)
        && string.Equals(organisationValue.Trim(), schoolValue.Trim(), StringComparison.OrdinalIgnoreCase);
}
