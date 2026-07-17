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
        public string SchoolDetails => $"{BasePath}/school-details";
        public string WhatIsASimilarSchool => $"{BasePath}/what-is-a-similar-school";
    }

    public class Secondary(string urn)
    {
        private string BasePath = $"/school/secondary/{urn}";

        public string Overview => BasePath;
        public string KS4HeadlineMeasures => $"{BasePath}/ks4-headline-measures";
        public string KS4HeadlineMeasuresData => $"{KS4HeadlineMeasures}/data";
        public string KS4CoreSubjects => $"{BasePath}/ks4-core-subjects";
        public string KS4CoreSubjectsData => $"{KS4CoreSubjects}/data";
        public string Attendance => $"{BasePath}/attendance";
        public string AttendanceData => $"{BasePath}/attendance-data";
        public string KS4DestinationsData => $"{BasePath}/ks4-destinations/data";
        public string ViewSimilarSchools => $"{BasePath}/view-similar-schools";
        public string SimilarSchoolComparison(string similarSchoolUrn)
            => $"{BasePath}/view-similar-schools/{similarSchoolUrn}";
        public string SchoolDetails => $"{BasePath}/school-details";
        public string WhatIsASimilarSchool => $"{BasePath}/what-is-a-similar-school";
        public SecondaryComparison Comparison(string similarSchoolUrn) => new(BasePath, similarSchoolUrn);
    }

    public class SecondaryComparison(string basePath, string similarSchoolUrn)
    {
        private string ComparisonBasePath => $"{basePath}/view-similar-schools/{similarSchoolUrn}";

        public string Overview => ComparisonBasePath;
        public string Similarity => $"{ComparisonBasePath}/Similarity";
        public string KS4HeadlineMeasures => $"{ComparisonBasePath}/Ks4HeadlineMeasures";
        public string KS4HeadlineMeasuresData => $"{ComparisonBasePath}/Ks4HeadlineMeasuresData";
        public string KS4CoreSubjects => $"{ComparisonBasePath}/Ks4CoreSubjects";
        public string KS4CoreSubjectsData => $"{ComparisonBasePath}/Ks4CoreSubjectsData";
        public string Attendance => $"{ComparisonBasePath}/attendance";
        public string AttendanceData => $"{ComparisonBasePath}/attendance-data";
        public string SchoolDetails => $"{ComparisonBasePath}/SchoolDetails";
        public string KS4DestinationsData => $"{ComparisonBasePath}/Ks4DestinationsData";
    }
}
