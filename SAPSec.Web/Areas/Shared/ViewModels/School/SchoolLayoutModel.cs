using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.Areas.Shared.ViewModels.School;

public record SchoolLayoutModel(string Urn, string Name)
{
    public static SchoolLayoutModel FromSchoolInfo(SchoolInfo schoolInfo) =>
        new(schoolInfo.Urn, schoolInfo.Name);

    public static SchoolLayoutModel FromSchoolDetails(SchoolDetails schoolDetails) =>
        new(schoolDetails.Urn, schoolDetails.Name);
}
