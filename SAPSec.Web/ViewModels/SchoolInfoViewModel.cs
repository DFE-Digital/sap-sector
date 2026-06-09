using SAPSec.Core.Features;

namespace SAPSec.Web.ViewModels;

public record SchoolInfoViewModel(string Urn, string Name)
{
    public static SchoolInfoViewModel FromSchoolInfo(SchoolInfo schoolInfo) =>
        new(schoolInfo.Urn, schoolInfo.Name);
}
