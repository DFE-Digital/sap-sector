using SAPSec.Core.Constants;

namespace SAPSec.Web.Constants;

public static class Routes
{
    public const string Home = "/";
    public const string Accessibility = "/";
    public const string SignIn = "/auth/signin";
    public const string Error = "/error";
    public const string AccessDenied = "/error/403";

    public static string FindASchool(string? query = null, int? page = null)
    {
        var queryString =
            (query is not null ? $"&query={query}" : "") +
            (page is not null ? $"&page={page}" : "");

        var qs = queryString.Any() ? "?" + queryString.Substring(1) : "";

        return $"/find-a-school{qs}";
    }

    public static string School(string urn) => $"/school/{urn}";
    public static string School(string urn, string? phaseOfEducationName) =>
        PhaseOfEducationValues.IsPrimaryOrAllThrough(phaseOfEducationName)
            ? $"/school/primary/{urn}"
            : School(urn);
    public static string SchoolDetails(string urn) => $"/school/{urn}/school-details";
    public static string SimilarSchools(string urn) => $"/school/{urn}/view-similar-schools";
    public static string SimilarSchoolComparison(string urn, string similarSchoolUrn) => $"/school/{urn}/view-similar-schools/{similarSchoolUrn}";
}

