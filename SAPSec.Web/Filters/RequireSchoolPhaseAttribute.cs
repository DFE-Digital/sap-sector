using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SAPSec.Core.Extensions;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Filters;

public enum ExpectedSchoolPhase
{
    Primary,
    Secondary
}

public sealed class RequireSchoolPhaseAttribute : TypeFilterAttribute
{
    public RequireSchoolPhaseAttribute(ExpectedSchoolPhase expectedPhase, params string[] routeParameterNames)
        : base(typeof(RequireSchoolPhaseFilter))
    {
        Arguments =
        [
            expectedPhase,
            routeParameterNames.Length > 0 ? routeParameterNames : ["urn"]
        ];
    }
}

public sealed class RequireSchoolPhaseFilter(
    ISchoolDetailsService schoolDetailsService,
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

            var school = await GetOrLoadSchoolAsync(context.HttpContext, urn, schoolDetailsService);
            if (!MatchesExpectedPhase(school, expectedPhase))
            {
                context.Result = new NotFoundResult();
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

    private static async Task<SchoolDetails> GetOrLoadSchoolAsync(
        HttpContext httpContext,
        string urn,
        ISchoolDetailsService schoolDetailsService)
    {
        if (ValidatedSchoolDetailsStore.TryGet(httpContext, urn, out var school))
        {
            return school;
        }

        school = await schoolDetailsService.GetByUrnAsync(urn);
        ValidatedSchoolDetailsStore.Set(httpContext, urn, school);
        return school;
    }
}

public static class ValidatedSchoolDetailsStore
{
    private const string CacheKey = "ValidatedSchoolDetails";

    public static bool TryGet(HttpContext? httpContext, string urn, out SchoolDetails school)
    {
        school = null!;

        if (httpContext is null)
        {
            return false;
        }

        if (httpContext.Items.TryGetValue(CacheKey, out var cachedValue)
            && cachedValue is Dictionary<string, SchoolDetails> cache
            && cache.TryGetValue(urn, out school!))
        {
            return true;
        }

        return false;
    }

    public static void Set(HttpContext? httpContext, string urn, SchoolDetails school)
    {
        if (httpContext is null)
        {
            return;
        }

        if (httpContext.Items.TryGetValue(CacheKey, out var cachedValue)
            && cachedValue is Dictionary<string, SchoolDetails> cache)
        {
            cache[urn] = school;
            return;
        }

        httpContext.Items[CacheKey] = new Dictionary<string, SchoolDetails>(StringComparer.OrdinalIgnoreCase)
        {
            [urn] = school
        };
    }
}
