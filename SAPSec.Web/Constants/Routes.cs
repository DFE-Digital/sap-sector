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

    public static string School(string urn, string? phaseOfEducationName) =>
        PhaseOfEducationValues.IsPrimaryOrAllThrough(phaseOfEducationName)
            ? PrimarySchool(urn).Home
            : SecondarySchool(urn).Home;

    public static Primary PrimarySchool(string urn) => new Primary(urn);
    public static Secondary SecondarySchool(string urn) => new Secondary(urn);

    public class Primary(string urn)
    {
        public string Home => $"/school/primary/{urn}";
        public string Ks2 => $"/school/primary/{urn}/ks2";
        public string Details => $"/school/primary/{urn}/school-details";
        public string SimilarSchools => $"/school/primary/{urn}/view-similar-schools";
        public string SimilarSchoolComparison(string similarSchoolUrn) => $"/school/primary/{urn}/view-similar-schools/{similarSchoolUrn}";
    }

    public class Secondary(string urn)
    {
        public string Home => $"/school/{urn}";
        public string Details => $"/school/{urn}/school-details";
        public string SimilarSchools => $"/school/{urn}/view-similar-schools";
        public string SimilarSchoolComparison(string similarSchoolUrn) => $"/school/{urn}/view-similar-schools/{similarSchoolUrn}";
    }
}
