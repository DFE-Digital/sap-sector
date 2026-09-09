using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public record ComparisonLayoutModel(string Urn, string Name, string SimilarSchoolUrn, string SimilarSchoolName)
{
    public static ComparisonLayoutModel FromSchoolInfo(SchoolInfo schoolInfo, SchoolInfo similarSchoolInfo) =>
        new(schoolInfo.Urn, schoolInfo.Name, similarSchoolInfo.Urn, similarSchoolInfo.Name);
}
