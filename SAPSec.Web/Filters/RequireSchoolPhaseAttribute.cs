using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Filters;

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
