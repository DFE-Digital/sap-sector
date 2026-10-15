using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public record ComparisonLayoutModel(
    SchoolInfoViewModel CurrentSchool,
    SchoolInfoViewModel ComparatorSchool,
    string ViewSimilarSchoolsUrl,
    string WhatIsASimilarSchoolUrl,
    IReadOnlyCollection<ComparisonNavigationItem> NavigationItems)
{
    public static ComparisonLayoutModel FromSchoolInfo(
        SchoolInfo currentSchoolInfo,
        SchoolInfo comparatorSchoolInfo,
        bool isCurrentSchoolAllThroughComparison = false) =>
        isCurrentSchoolAllThroughComparison
            ? AllThroughSecondary(currentSchoolInfo, comparatorSchoolInfo)
            : Secondary(currentSchoolInfo, comparatorSchoolInfo);

    public static ComparisonLayoutModel Primary(SchoolInfo currentSchoolInfo, SchoolInfo comparatorSchoolInfo) =>
        Create(
            currentSchoolInfo,
            comparatorSchoolInfo,
            Routes.PrimarySchool(currentSchoolInfo.Urn).ViewSimilarSchools,
            Routes.PrimarySchool(currentSchoolInfo.Urn).WhatIsASimilarSchool,
            [
                new("Similarity", Routes.PrimarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).Similarity, ["Index", "Similarity"]),
                new("KS2", Routes.PrimarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).Ks2, ["Ks2PerformanceMeasures"]),
                new("Attendance", Routes.PrimarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).Attendance, ["Attendance"]),
                new("School details", Routes.PrimarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).SchoolDetails, ["SchoolDetails"])
            ]);

    public static ComparisonLayoutModel Secondary(SchoolInfo currentSchoolInfo, SchoolInfo comparatorSchoolInfo) =>
        Create(
            currentSchoolInfo,
            comparatorSchoolInfo,
            Routes.SecondarySchool(currentSchoolInfo.Urn).ViewSimilarSchools,
            Routes.SecondarySchool(currentSchoolInfo.Urn).WhatIsASimilarSchool,
            [
                new("Similarity", Routes.SecondarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).Similarity, ["Similarity"]),
                new("KS4 headline measures", Routes.SecondarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).KS4HeadlineMeasures, ["Ks4HeadlineMeasures"]),
                new("KS4 core subjects", Routes.SecondarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).KS4CoreSubjects, ["Ks4CoreSubjects"]),
                new("Attendance", Routes.SecondarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).Attendance, ["Attendance"]),
                new("School details", Routes.SecondarySchool(currentSchoolInfo.Urn).Comparison(comparatorSchoolInfo.Urn).SchoolDetails, ["SchoolDetails"])
            ]);

    public static ComparisonLayoutModel AllThroughPrimary(SchoolInfo currentSchoolInfo, SchoolInfo comparatorSchoolInfo) =>
        Create(
            currentSchoolInfo,
            comparatorSchoolInfo,
            Routes.AllThroughSchool(currentSchoolInfo.Urn).ViewSimilarSchools,
            Routes.AllThroughSchool(currentSchoolInfo.Urn).WhatIsASimilarSchool,
            [
                new("Similarity", Routes.AllThroughSchool(currentSchoolInfo.Urn).PrimaryComparison(comparatorSchoolInfo.Urn).Similarity, ["PrimarySimilarity"]),
                new("KS2", Routes.AllThroughSchool(currentSchoolInfo.Urn).PrimaryComparison(comparatorSchoolInfo.Urn).Ks2, ["PrimaryKs2PerformanceMeasures"]),
                new("Attendance", Routes.AllThroughSchool(currentSchoolInfo.Urn).PrimaryComparison(comparatorSchoolInfo.Urn).Attendance, ["PrimaryAttendance"]),
                new("School details", Routes.AllThroughSchool(currentSchoolInfo.Urn).PrimaryComparison(comparatorSchoolInfo.Urn).SchoolDetails, ["PrimarySchoolDetails"])
            ]);

    public static ComparisonLayoutModel AllThroughSecondary(SchoolInfo currentSchoolInfo, SchoolInfo comparatorSchoolInfo) =>
        Create(
            currentSchoolInfo,
            comparatorSchoolInfo,
            $"{Routes.AllThroughSchool(currentSchoolInfo.Urn).ViewSimilarSchools}?phase=secondary",
            Routes.AllThroughSchool(currentSchoolInfo.Urn).WhatIsASimilarSchool,
            [
                new("Similarity", Routes.AllThroughSchool(currentSchoolInfo.Urn).SecondaryComparison(comparatorSchoolInfo.Urn).Similarity, ["SecondarySimilarity"]),
                new("KS4 headline measures", Routes.AllThroughSchool(currentSchoolInfo.Urn).SecondaryComparison(comparatorSchoolInfo.Urn).KS4HeadlineMeasures, ["SecondaryKs4HeadlineMeasures"]),
                new("KS4 core subjects", Routes.AllThroughSchool(currentSchoolInfo.Urn).SecondaryComparison(comparatorSchoolInfo.Urn).KS4CoreSubjects, ["SecondaryKs4CoreSubjects"]),
                new("Attendance", Routes.AllThroughSchool(currentSchoolInfo.Urn).SecondaryComparison(comparatorSchoolInfo.Urn).Attendance, ["SecondaryAttendance"]),
                new("School details", Routes.AllThroughSchool(currentSchoolInfo.Urn).SecondaryComparison(comparatorSchoolInfo.Urn).SchoolDetails, ["SecondarySchoolDetails"])
            ]);

    private static ComparisonLayoutModel Create(
        SchoolInfo currentSchoolInfo,
        SchoolInfo comparatorSchoolInfo,
        string viewSimilarSchoolsUrl,
        string whatIsASimilarSchoolUrl,
        IReadOnlyCollection<ComparisonNavigationItem> navigationItems) =>
        new(
            SchoolInfoViewModel.FromSchoolInfo(currentSchoolInfo),
            SchoolInfoViewModel.FromSchoolInfo(comparatorSchoolInfo),
            viewSimilarSchoolsUrl,
            whatIsASimilarSchoolUrl,
            navigationItems);
}

public record ComparisonNavigationItem(string Text, string Href, IReadOnlyCollection<string> ActiveActionNames);
