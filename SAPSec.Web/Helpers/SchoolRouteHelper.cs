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
        => ReplaceSchoolPath(requestPath, urn, Routes.PrimarySchool(urn).Overview);

    private static string GetSecondaryPath(PathString requestPath, string urn)
        => ReplaceSchoolPath(requestPath, urn, Routes.SecondarySchool(urn).Overview);

    private static string ReplaceSchoolPath(PathString requestPath, string urn, string targetBasePath)
    {
        var path = requestPath.Value ?? string.Empty;
        var normalizedPath = path.EndsWith("/similar-schools", StringComparison.OrdinalIgnoreCase)
            ? path[..^"/similar-schools".Length] + "/view-similar-schools"
            : path;
        var primaryRoutes = Routes.PrimarySchool(urn);
        var secondaryRoutes = Routes.SecondarySchool(urn);

        foreach (var sourceBasePath in new[]
        {
            $"/school/{urn}",
            primaryRoutes.Overview,
            secondaryRoutes.Overview
        })
        {
            if (normalizedPath.StartsWith(sourceBasePath, StringComparison.OrdinalIgnoreCase))
            {
                var candidatePath = targetBasePath + normalizedPath[sourceBasePath.Length..];
                return IsSupportedTargetPath(candidatePath, primaryRoutes, secondaryRoutes)
                    ? candidatePath
                    : targetBasePath;
            }
        }

        return targetBasePath;
    }

    private static bool IsSupportedTargetPath(string candidatePath, Routes.Primary primaryRoutes, Routes.Secondary secondaryRoutes)
    {
        var supportedPaths = candidatePath.StartsWith(primaryRoutes.Overview, StringComparison.OrdinalIgnoreCase)
            ? new[]
            {
                primaryRoutes.Overview,
                primaryRoutes.Attendance,
                primaryRoutes.SchoolDetails,
                primaryRoutes.WhatIsASimilarSchool,
                primaryRoutes.ViewSimilarSchools
            }
            : new[]
            {
                secondaryRoutes.Overview,
                secondaryRoutes.Attendance,
                secondaryRoutes.AttendanceData,
                secondaryRoutes.SchoolDetails,
                secondaryRoutes.WhatIsASimilarSchool,
                secondaryRoutes.ViewSimilarSchools,
                secondaryRoutes.KS4HeadlineMeasures,
                secondaryRoutes.KS4HeadlineMeasuresData,
                secondaryRoutes.KS4DestinationsData,
                secondaryRoutes.KS4CoreSubjects,
                secondaryRoutes.KS4CoreSubjectsData
            };

        return supportedPaths.Any(path => candidatePath.Equals(path, StringComparison.OrdinalIgnoreCase));
    }
}
