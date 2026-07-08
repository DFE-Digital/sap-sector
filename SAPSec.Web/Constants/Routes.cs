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
        public string KS2 => $"/school/primary/{urn}/ks2";
        public string Attendance => $"/school/primary/{urn}/attendance";
        public string ViewSimilarSchools => $"/school/primary/{urn}/view-similar-schools";
        public string SimilarSchoolComparison(string similarSchoolUrn)
            => $"/school/primary/{urn}/view-similar-schools/{similarSchoolUrn}";
        public string SchoolDetails => $"/school/primary/{urn}/school-details";
        public string WhatIsASimilarSchool => $"/school/primary/{urn}/what-is-a-similar-school";
    }

    public class Secondary(string urn)
    {
        public string Home => $"/school/{urn}";
        public string KS4HeadlineMeasures => $"/school/{urn}/ks4-headline-measures";
        public string KS4CoreSubjects => $"/school/{urn}/ks4-core-subjects";
        public string Attendance => $"/school/{urn}/attendance";
        public string ViewSimilarSchools => $"/school/{urn}/view-similar-schools";
        public string SimilarSchoolComparison(string similarSchoolUrn)
            => $"/school/{urn}/view-similar-schools/{similarSchoolUrn}";
        public string SchoolDetails => $"/school/{urn}/school-details";
        public string WhatIsASimilarSchool => $"/school/{urn}/what-is-a-similar-school";
    }
}
