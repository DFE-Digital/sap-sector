namespace SAPSec.Web.Areas.Shared.ViewModels.School;

public record SchoolLayoutModel(string Urn, string Name)
{
    public static SchoolLayoutModel FromSchool(Core.Features.SchoolInfo.School school) =>
        new(school.Urn, school.Name);
}
