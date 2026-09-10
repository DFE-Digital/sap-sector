using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public record ComparisonLayoutModel(SchoolInfoViewModel CurrentSchool, SchoolInfoViewModel ComparatorSchool)
{
    public static ComparisonLayoutModel FromSchoolInfo(SchoolInfo currentSchoolInfo, SchoolInfo comparatorSchoolInfo) =>
        new(SchoolInfoViewModel.FromSchoolInfo(currentSchoolInfo), SchoolInfoViewModel.FromSchoolInfo(comparatorSchoolInfo));
}
