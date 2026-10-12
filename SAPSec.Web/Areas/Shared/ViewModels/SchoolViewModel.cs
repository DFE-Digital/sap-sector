namespace SAPSec.Web.Areas.Shared.ViewModels;

public record SchoolViewModel(string Urn, string Name, string Address)
{
    public static SchoolViewModel FromSchool(Core.Features.SchoolInfo.School school) =>
        new(school.Urn, school.Name, school.Address.ToString());
}
