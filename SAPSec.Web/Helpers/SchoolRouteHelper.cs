using Microsoft.AspNetCore.Http;
using SAPSec.Core.Extensions;
using SAPSec.Core.Model;
using SAPSec.Web.Constants;

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
        => (requestPath.Value ?? string.Empty)
            .Replace($"/school/{urn}", Routes.PrimarySchool(urn).Overview, StringComparison.OrdinalIgnoreCase)
            .Replace(Routes.SecondarySchool(urn).Overview, Routes.PrimarySchool(urn).Overview, StringComparison.OrdinalIgnoreCase);

    private static string GetSecondaryPath(PathString requestPath, string urn)
        => (requestPath.Value ?? string.Empty)
            .Replace($"/school/{urn}", Routes.SecondarySchool(urn).Overview, StringComparison.OrdinalIgnoreCase)
            .Replace(Routes.PrimarySchool(urn).Overview, Routes.SecondarySchool(urn).Overview, StringComparison.OrdinalIgnoreCase);
}
