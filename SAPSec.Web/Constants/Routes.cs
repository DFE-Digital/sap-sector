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
            ? PrimarySchool(urn).Overview
            : SecondarySchool(urn).Overview;

    public static Primary PrimarySchool(string urn) => new Primary(urn);
    public static Secondary SecondarySchool(string urn) => new Secondary(urn);

    public class Primary(string urn)
    {
        private string BasePath = $"/school/primary/{urn}";

        public string Overview => BasePath;
        public string KS2 => $"{BasePath}/ks2";
        public string Attendance => $"{BasePath}/attendance";
        public string ViewSimilarSchools => $"{BasePath}/view-similar-schools";
        public string SimilarSchoolComparison(string similarSchoolUrn)
            => $"{BasePath}/view-similar-schools/{similarSchoolUrn}";
        public string SimilarSchoolComparisonKs2(string similarSchoolUrn)
            => $"{SimilarSchoolComparison(similarSchoolUrn)}/ks2";
        public string SimilarSchoolComparisonAttendance(string similarSchoolUrn)
            => $"{SimilarSchoolComparison(similarSchoolUrn)}/attendance";
        public string SimilarSchoolComparisonSchoolDetails(string similarSchoolUrn)
            => $"{SimilarSchoolComparison(similarSchoolUrn)}/school-details";
        public string SchoolDetails => $"{BasePath}/school-details";
        public string WhatIsASimilarSchool => $"{BasePath}/what-is-a-similar-school";
    }

    public class Secondary(string urn)
    {
        private string BasePath = $"/school/{urn}";

        public string Overview => BasePath;
        public string KS4HeadlineMeasures => $"{BasePath}/ks4-headline-measures";
        public string KS4CoreSubjects => $"{BasePath}/ks4-core-subjects";
        public string Attendance => $"{BasePath}/attendance";
        public string ViewSimilarSchools => $"{BasePath}/view-similar-schools";
        public string SimilarSchoolComparison(string similarSchoolUrn)
            => $"{BasePath}/view-similar-schools/{similarSchoolUrn}";
        public string SchoolDetails => $"{BasePath}/school-details";
        public string WhatIsASimilarSchool => $"{BasePath}/what-is-a-similar-school";
    }
}
