using SAPSec.Core.School.Info;

namespace SAPSec.Web.ViewModels;

public record SchoolInfoViewModel(string Urn, string Name, string Address)
{
    public static SchoolInfoViewModel FromSchoolInfo(SchoolInfo schoolInfo) =>
        new(schoolInfo.Urn, schoolInfo.Name, schoolInfo.Address.ToString());
}
