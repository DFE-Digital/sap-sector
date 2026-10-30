using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Controllers;

[Authorize]
[Route("user")]
public class UserController(
    IUserService userService,
    IFeatureFlagService featureFlagService,
    ILogger<UserController> logger) : Controller
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly IFeatureFlagService _featureFlagService = featureFlagService ?? throw new ArgumentNullException(nameof(featureFlagService));
    private readonly ILogger<UserController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet]
    [Route("redirect")]
    public async Task<IActionResult> Index()
    {
        var traceId = HttpContext.TraceIdentifier;
        var userName = User.Identity?.Name ?? "(unknown)";
        var requestPath = HttpContext.Request.Path.Value ?? "(unknown)";

        _logger.LogInformation(
            "Entered /user/redirect. TraceId: {TraceId}. Path: {Path}. IsAuthenticated: {IsAuthenticated}. UserName: {UserName}.",
            traceId,
            requestPath,
            User.Identity?.IsAuthenticated == true,
            userName);

        var user = await _userService.GetUserFromClaimsAsync(User);
        if (user is null)
        {
            _logger.LogError(
                "Unable to create user object from claims in /user/redirect. TraceId: {TraceId}. Path: {Path}. UserName: {UserName}.",
                traceId,
                requestPath,
                userName);
            return Redirect(Routes.AccessDenied);
        }

        var userId = user.Sub;

        _logger.LogInformation(
            "Resolved user in /user/redirect. TraceId: {TraceId}. UserId: {UserId}. OrganisationCount: {OrganisationCount}.",
            traceId,
            userId,
            user.Organisations.Count);

        var org = await _userService.GetCurrentOrganisationAsync(User);

        if (await _featureFlagService.IsEnabledAsync(Flags.EnableFindASchool))
        {
            return RedirectWithoutRestrictions(org, userId);
        }

        if (org is { IsEstablishment: true })
        {
            return RedirectToSchool(org, traceId, userId);
        }

        // Further starting pages (e.g. single academy trust, internal DfE users who may have no organisation)
        // are added as branches above this.
        if (org is null)
        {
            _logger.LogError(
                "Current organisation was null in /user/redirect. TraceId: {TraceId}. UserId: {UserId}.",
                traceId,
                userId);
            throw new InvalidOperationException($"Current organisation is null for user {userId}.");
        }

        _logger.LogWarning(
            "No starting page for user in /user/redirect; returning forbidden. TraceId: {TraceId}. UserId: {UserId}. OrganisationId: {OrganisationId}. OrganisationName: {OrganisationName}. Category: {OrganisationCategory}. Urn: {OrganisationUrn}.",
            traceId,
            userId,
            org.Id,
            org.Name,
            org.Category?.Name,
            org.Urn);
        return Forbid();
    }

    // How users were routed before the school user rules, kept while they are behind a feature flag.
    private IActionResult RedirectWithoutRestrictions(Organisation? org, string userId)
    {
        if (org is null)
        {
            _logger.LogError("Current organisation was null in /user/redirect. UserId: {UserId}.", userId);
            throw new InvalidOperationException($"Current organisation is null for user {userId}.");
        }

        return !org.IsEstablishment || org.Urn is null
            ? Redirect(Routes.FindASchool())
            : Redirect(Routes.School(org.Urn, org.PhaseOfEducation?.Name));
    }

    private IActionResult RedirectToSchool(Organisation org, string traceId, string userId)
    {
        if (org.Urn is null)
        {
            _logger.LogWarning(
                "School user has no URN in /user/redirect. TraceId: {TraceId}. UserId: {UserId}. OrganisationId: {OrganisationId}.",
                traceId,
                userId,
                org.Id);
            return NotFound();
        }

        _logger.LogInformation(
            "Redirecting from /user/redirect to school page. TraceId: {TraceId}. UserId: {UserId}. OrganisationId: {OrganisationId}. OrganisationName: {OrganisationName}. Urn: {OrganisationUrn}. RedirectTarget: {RedirectTarget}.",
            traceId,
            userId,
            org.Id,
            org.Name,
            org.Urn,
            Routes.School(org.Urn, org.PhaseOfEducation?.Name));

        return Redirect(Routes.School(org.Urn, org.PhaseOfEducation?.Name));
    }
}
