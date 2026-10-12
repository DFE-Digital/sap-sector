using SAPSec.Web.Constants;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public record ComparisonLayoutModel(
    SchoolViewModel CurrentSchool,
    SchoolViewModel ComparatorSchool,
    string ViewSimilarSchoolsUrl,
    string WhatIsASimilarSchoolUrl,
    IReadOnlyCollection<ComparisonNavigationItem> NavigationItems)
{
    public static ComparisonLayoutModel FromSchoolInfo(
        Core.Features.SchoolInfo.School currentSchoolInfo,
        Core.Features.SchoolInfo.School comparatorSchoolInfo,
        bool isCurrentSchoolAllThroughComparison = false) =>
        isCurrentSchoolAllThroughComparison
            ? AllThroughSecondary(currentSchoolInfo, comparatorSchoolInfo)
            : Secondary(currentSchoolInfo, comparatorSchoolInfo);

    public static ComparisonLayoutModel Primary(Core.Features.SchoolInfo.School currentSchoolInfo, Core.Features.SchoolInfo.School comparatorSchoolInfo) =>
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

    public static ComparisonLayoutModel Secondary(Core.Features.SchoolInfo.School currentSchoolInfo, Core.Features.SchoolInfo.School comparatorSchoolInfo) =>
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

    public static ComparisonLayoutModel AllThroughPrimary(Core.Features.SchoolInfo.School currentSchoolInfo, Core.Features.SchoolInfo.School comparatorSchoolInfo) =>
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

    public static ComparisonLayoutModel AllThroughSecondary(Core.Features.SchoolInfo.School currentSchoolInfo, Core.Features.SchoolInfo.School comparatorSchoolInfo) =>
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
        Core.Features.SchoolInfo.School currentSchoolInfo,
        Core.Features.SchoolInfo.School comparatorSchoolInfo,
        string viewSimilarSchoolsUrl,
        string whatIsASimilarSchoolUrl,
        IReadOnlyCollection<ComparisonNavigationItem> navigationItems) =>
        new(
            SchoolViewModel.FromSchool(currentSchoolInfo),
            SchoolViewModel.FromSchool(comparatorSchoolInfo),
            viewSimilarSchoolsUrl,
            whatIsASimilarSchoolUrl,
            navigationItems);
}

public record ComparisonNavigationItem(string Text, string Href, IReadOnlyCollection<string> ActiveActionNames);
