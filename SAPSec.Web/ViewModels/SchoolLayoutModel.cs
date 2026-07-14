using SAPSec.Core.School.Info;

namespace SAPSec.Web.ViewModels;

public record SchoolLayoutModel(string Urn, string Name)
{
    public static SchoolLayoutModel FromSchoolInfo(SchoolInfo schoolInfo) =>
        new(schoolInfo.Urn, schoolInfo.Name);
}
