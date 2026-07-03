using Microsoft.AspNetCore.Http;
using SAPSec.Core.Extensions;
using SAPSec.Core.Model;

namespace SAPSec.Web.Helpers;

public static class SchoolRouteHelper
{
    public static bool TryGetPhaseRedirectPath(
        PathString requestPath,
        SchoolDetails school,
        PathString pathBase,
        out string redirectPath)
    {
        ArgumentNullException.ThrowIfNull(school);

        var relativePath = school switch
        {
            _ when school.IsPrimarySchool() => GetPrimaryPath(requestPath, school.Urn),
            _ when school.IsSecondarySchool() => GetSecondaryPath(requestPath, school.Urn),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            redirectPath = string.Empty;
            return false;
        }

        redirectPath = $"{pathBase}{relativePath}";
        return true;
    }

    private static string GetPrimaryPath(PathString requestPath, string urn)
    {
        var path = requestPath.Value ?? string.Empty;

        return path.ToLowerInvariant() switch
        {
            var p when p.EndsWith("/attendance") => $"/school/primary/{urn}/attendance",
            var p when p.EndsWith("/school-details") => $"/school/primary/{urn}/school-details",
            var p when p.EndsWith("/what-is-a-similar-school") => $"/school/primary/{urn}/what-is-a-similar-school",
            var p when p.EndsWith("/view-similar-schools") => $"/school/primary/{urn}/view-similar-schools",
            var p when p.EndsWith("/similar-schools") => $"/school/primary/{urn}/view-similar-schools",
            _ => $"/school/primary/{urn}"
        };
    }

    private static string GetSecondaryPath(PathString requestPath, string urn)
    {
        var path = requestPath.Value ?? string.Empty;

        return path.ToLowerInvariant() switch
        {
            var p when p.EndsWith("/attendance") => $"/school/{urn}/attendance",
            var p when p.EndsWith("/school-details") => $"/school/{urn}/school-details",
            var p when p.EndsWith("/what-is-a-similar-school") => $"/school/{urn}/what-is-a-similar-school",
            var p when p.EndsWith("/view-similar-schools") => $"/school/{urn}/view-similar-schools",
            var p when p.EndsWith("/similar-schools") => $"/school/{urn}/view-similar-schools",
            _ => $"/school/{urn}"
        };
    }
}
