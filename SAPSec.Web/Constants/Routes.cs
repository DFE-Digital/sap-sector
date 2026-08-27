using SAPSec.Core.Constants;

namespace SAPSec.Web.Constants;

public static class Routes
{
    public const string Home = "/";
    public const string Accessibility = "/";
    public const string SignIn = "/auth/signin";
    public const string Error = "/error";
    public const string AccessDenied = "/error/403";
    public const string TermsAndConditions = "/terms-and-conditions";

    public const string FindASchoolBasePath = "/find-a-school";
    public static string FindASchool(string? query = null, string? page = null, string[]? localAuthorities = null)
    {
        var queryString =
            (query is not null ? $"&query={Uri.EscapeDataString(query)}" : "") +
            (page is not null ? $"&page={Uri.EscapeDataString(page)}" : "");

        if (localAuthorities is not null)
        {
            foreach (var la in localAuthorities)
            {
                queryString += $"&localAuthorities={Uri.EscapeDataString(la)}";
            }
        }

        var qs = queryString.Any() ? "?" + queryString.Substring(1) : "";

        return $"/find-a-school{qs}";
    }

    public static string FindASchoolSuggest(string? queryPart = null)
    {
        var queryString =
            (queryPart is not null ? $"&queryPart={Uri.EscapeDataString(queryPart)}" : "");

        var qs = queryString.Any() ? "?" + queryString.Substring(1) : "";

        return $"/find-a-school/suggest{qs}";
    }

    public static string School(string urn, string? phaseOfEducationName) =>
        PhaseOfEducationValues.IsPrimaryOrAllThrough(phaseOfEducationName)
            ? PrimarySchool(urn).Overview
            : SecondarySchool(urn).Overview;

    public static Primary PrimarySchool(string urn) => new Primary(urn);
    public static Secondary SecondarySchool(string urn) => new Secondary(urn);

    public class Primary(string urn)
    {
        private string _basePath = $"/school/primary/{urn}";

        public string Overview => _basePath;
        public string KS2 => $"{_basePath}/ks2";
        public string Attendance => $"{_basePath}/attendance";
        public string ViewSimilarSchools => $"{_basePath}/view-similar-schools";
        public string SchoolDetails => $"{_basePath}/school-details";
        public string WhatIsASimilarSchool => $"{_basePath}/what-is-a-similar-school";
        public PrimaryComparison Comparison(string similarSchoolUrn) => new PrimaryComparison(_basePath, similarSchoolUrn);

        public class PrimaryComparison(string basePath, string similarSchoolUrn)
        {
            private string _basePath = $"{basePath}/view-similar-schools/{similarSchoolUrn}";

            public string BasePath => _basePath;
            public string Similarity => $"{_basePath}/compare-similarity";
            public string Ks2 => $"{_basePath}/compare-ks2";
            public string Attendance => $"{_basePath}/compare-attendance";
            public string SchoolDetails => $"{_basePath}/compare-school-details";
        }
    }

    public class Secondary(string urn)
    {
        private string _basePath = $"/school/secondary/{urn}";

        public string Overview => _basePath;
        public string KS4HeadlineMeasures => $"{_basePath}/ks4-headline-measures";
        public string KS4CoreSubjects => $"{_basePath}/ks4-core-subjects";
        public string Attendance => $"{_basePath}/attendance";
        public string AttendanceData => $"{_basePath}/attendance-data";
        public string ViewSimilarSchools => $"{_basePath}/view-similar-schools";
        public string SchoolDetails => $"{_basePath}/school-details";
        public string WhatIsASimilarSchool => $"{_basePath}/what-is-a-similar-school";
        public SecondaryComparison Comparison(string similarSchoolUrn) => new(_basePath, similarSchoolUrn);

        public class SecondaryComparison(string basePath, string similarSchoolUrn)
        {
            private string _basePath => $"{basePath}/view-similar-schools/{similarSchoolUrn}";

            public string BasePath => _basePath;
            public string Similarity => $"{_basePath}/compare-similarity";
            public string KS4HeadlineMeasures => $"{_basePath}/compare-ks4-headline-measures";
            public string KS4CoreSubjects => $"{_basePath}/compare-ks4-core-subjects";
            public string Attendance => $"{_basePath}/compare-attendance";
            public string AttendanceData => $"{_basePath}/attendance-data";
            public string SchoolDetails => $"{_basePath}/compare-school-details";
        }
    }
}
