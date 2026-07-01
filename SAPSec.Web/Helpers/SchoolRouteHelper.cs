using Microsoft.AspNetCore.Http;
using SAPSec.Core.Extensions;
using SAPSec.Core.Model;

namespace SAPSec.Web.Helpers;

public static class SchoolRouteHelper
{
    public static bool TryGetPhaseRedirectPath(
        string? controller,
        string? action,
        SchoolDetails school,
        PathString pathBase,
        out string redirectPath)
    {
        ArgumentNullException.ThrowIfNull(school);

        var relativePath = school switch
        {
            _ when school.IsPrimarySchool() => GetPrimaryPath(controller, action, school.Urn),
            _ when school.IsSecondarySchool() => GetSecondaryPath(controller, action, school.Urn),
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

    private static string GetPrimaryPath(string? controller, string? action, string urn) =>
        (controller, action) switch
        {
            ("School", "Attendance") => $"/school/primary/{urn}/attendance",
            ("School", "SchoolDetails") => $"/school/primary/{urn}/school-details",
            ("School", "WhatIsASimilarSchool") => $"/school/primary/{urn}/what-is-a-similar-school",
            ("School", "ViewSimilarSchools") => $"/school/primary/{urn}/view-similar-schools",
            ("SimilarSchools", "ViewSimilarSchools") => $"/school/primary/{urn}/view-similar-schools",
            ("SimilarSchools", "Index") => $"/school/primary/{urn}/view-similar-schools",
            _ => $"/school/primary/{urn}"
        };

    private static string GetSecondaryPath(string? controller, string? action, string urn) =>
        (controller, action) switch
        {
            ("School", "Attendance") => $"/school/{urn}/attendance",
            ("School", "SchoolDetails") => $"/school/{urn}/school-details",
            ("School", "WhatIsASimilarSchool") => $"/school/{urn}/what-is-a-similar-school",
            ("School", "ViewSimilarSchools") => $"/school/{urn}/view-similar-schools",
            ("SimilarSchools", "ViewSimilarSchools") => $"/school/{urn}/view-similar-schools",
            ("SimilarSchools", "Index") => $"/school/{urn}/view-similar-schools",
            _ => $"/school/{urn}"
        };
}
