using Microsoft.AspNetCore.Http;
using SAPSec.Core.Extensions;
using SAPSec.Core.Model;

namespace SAPSec.Web.Helpers;

public static class SchoolRouteHelper
{
    public static bool TryGetPhaseOverviewPath(
        SchoolDetails school,
        PathString pathBase,
        out string overviewPath)
    {
        ArgumentNullException.ThrowIfNull(school);

        var relativePath = school switch
        {
            _ when school.IsPrimarySchool() => $"/school/primary/{school.Urn}",
            _ when school.IsSecondarySchool() => $"/school/{school.Urn}",
            _ => null
        };

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            overviewPath = string.Empty;
            return false;
        }

        overviewPath = $"{pathBase}{relativePath}";
        return true;
    }
}
