using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Controllers;

[Authorize]
[Route("user")]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger) : Controller
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
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
        if (org is null)
        {
            _logger.LogError(
                "Current organisation was null in /user/redirect. TraceId: {TraceId}. UserId: {UserId}.",
                traceId,
                userId);
            throw new InvalidOperationException($"Current organisation is null for user {userId}.");
        }

        if (!org.IsEstablishment || org.Urn is null)
        {
            _logger.LogInformation(
                "Redirecting from /user/redirect to find-a-school. TraceId: {TraceId}. UserId: {UserId}. OrganisationId: {OrganisationId}. OrganisationName: {OrganisationName}. Category: {OrganisationCategory}. Urn: {OrganisationUrn}.",
                traceId,
                userId,
                org.Id,
                org.Name,
                org.Category?.Name,
                org.Urn);
            return Redirect(Routes.FindASchool);
        }

        _logger.LogInformation(
            "Redirecting from /user/redirect to school page. TraceId: {TraceId}. UserId: {UserId}. OrganisationId: {OrganisationId}. OrganisationName: {OrganisationName}. Urn: {OrganisationUrn}. RedirectTarget: {RedirectTarget}.",
            traceId,
            userId,
            org.Id,
            org.Name,
            org.Urn,
            Routes.School(org.Urn));

        return Redirect(Routes.School(org.Urn));
    }
}
