using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Model;

namespace SAPSec.Web.Areas.Shared.ViewModels;

public record SchoolInfoViewModel(string Urn, string Name, string Address)
{
    public static SchoolInfoViewModel FromSchoolInfo(SchoolInfo schoolInfo) =>
        new(schoolInfo.Urn, schoolInfo.Name, schoolInfo.Address.ToString());

    public static SchoolInfoViewModel FromSchoolDetails(SchoolDetails schoolDetails) =>
        new(schoolDetails.Urn, schoolDetails.Name, schoolDetails.Address.GetValueOrDefault(""));
}
