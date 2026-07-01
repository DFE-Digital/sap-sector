using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SAPSec.Core.Extensions;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Helpers;

namespace SAPSec.Web.Filters;

public sealed class RequireSchoolPhaseFilter(
    IRequestSchoolAccessor requestSchoolAccessor,
    ExpectedSchoolPhase expectedPhase,
    string[] routeParameterNames) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var routeParameterName in routeParameterNames)
        {
            if (!TryGetRouteValue(context, routeParameterName, out var urn))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var school = await requestSchoolAccessor.GetAsync(context.HttpContext, urn);
            if (!MatchesExpectedPhase(school, expectedPhase))
            {
                context.Result = TryBuildRedirectResult(context, school);
                return;
            }
        }

        await next();
    }

    private static bool TryGetRouteValue(ActionExecutingContext context, string routeParameterName, out string urn)
    {
        urn = string.Empty;

        if (!context.RouteData.Values.TryGetValue(routeParameterName, out var routeValue))
        {
            return false;
        }

        urn = routeValue?.ToString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(urn);
    }

    private static bool MatchesExpectedPhase(SchoolDetails school, ExpectedSchoolPhase expectedPhase)
        => expectedPhase switch
        {
            ExpectedSchoolPhase.Primary => school.IsPrimarySchool(),
            ExpectedSchoolPhase.Secondary => school.IsSecondarySchool(),
            _ => false
        };

    private IActionResult TryBuildRedirectResult(ActionExecutingContext context, SchoolDetails school)
    {
        if (routeParameterNames.Length != 1)
        {
            return new NotFoundResult();
        }

        if (!SchoolRouteHelper.TryGetPhaseOverviewPath(
                school,
                context.HttpContext.Request.PathBase,
                out var canonicalPath))
        {
            return new NotFoundResult();
        }

        return new RedirectResult(canonicalPath + context.HttpContext.Request.QueryString, permanent: false);
    }
}
