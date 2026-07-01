using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SAPSec.Core.Extensions;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

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

        var request = context.HttpContext.Request;
        var canonicalPath = school.IsPrimarySchool()
            ? BuildPrimaryPath(context, school.Urn)
            : BuildSecondaryPath(context, school.Urn);

        return new RedirectResult(canonicalPath + request.QueryString, permanent: false);
    }

    private static string BuildPrimaryPath(ActionExecutingContext context, string urn)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();

        if (string.Equals(controller, "SimilarSchools", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(action, "ViewSimilarSchools", StringComparison.OrdinalIgnoreCase))
        {
            return $"/school/primary/{urn}/view-similar-schools";
        }

        return action switch
        {
            "Ks2" => $"/school/primary/{urn}/ks2",
            "Attendance" => $"/school/primary/{urn}/attendance",
            "SchoolDetails" => $"/school/primary/{urn}/school-details",
            "WhatIsASimilarSchool" => $"/school/primary/{urn}/what-is-a-similar-school",
            _ => $"/school/primary/{urn}"
        };
    }

    private static string BuildSecondaryPath(ActionExecutingContext context, string urn)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();

        if (string.Equals(controller, "SimilarSchools", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(action, "ViewSimilarSchools", StringComparison.OrdinalIgnoreCase))
        {
            return $"/school/{urn}/view-similar-schools";
        }

        return action switch
        {
            "Attendance" => $"/school/{urn}/attendance",
            "SchoolDetails" => $"/school/{urn}/school-details",
            "WhatIsASimilarSchool" => $"/school/{urn}/what-is-a-similar-school",
            "Ks4HeadlineMeasures" => $"/school/{urn}/ks4-headline-measures",
            "Ks4CoreSubjects" => $"/school/{urn}/ks4-core-subjects",
            _ => $"/school/{urn}"
        };
    }
}
