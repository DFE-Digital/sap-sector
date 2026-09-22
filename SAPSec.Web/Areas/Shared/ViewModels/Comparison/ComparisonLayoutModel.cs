using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public record ComparisonLayoutModel(
    SchoolInfoViewModel CurrentSchool,
    SchoolInfoViewModel ComparatorSchool,
    bool IsCurrentSchoolAllThroughComparison = false)
{
    public static ComparisonLayoutModel FromSchoolInfo(
        SchoolInfo currentSchoolInfo,
        SchoolInfo comparatorSchoolInfo,
        bool isCurrentSchoolAllThroughComparison = false) =>
        new(
            SchoolInfoViewModel.FromSchoolInfo(currentSchoolInfo),
            SchoolInfoViewModel.FromSchoolInfo(comparatorSchoolInfo),
            isCurrentSchoolAllThroughComparison);
}
