using SAPSec.Core.Model;
using static SAPSec.Web.ViewModels.Ks4CoreSubjectsPageViewModel;

namespace SAPSec.Web.ViewModels;

public class MeasureViewModel
{
    public required string Key { get; set; }
    public required string HtmlPrefix { get; set; }
    public required string Title { get; set; }
    public required SchoolDetails SchoolDetails { get; set; }
    public required SubjectSection Subject { get; set; }
    public required IEnumerable<GradeOptionViewModel> GradeOptions { get; set; }

    public string SchoolLabel => SchoolDetails.Name;
    public string SimilarSchoolsLabel => "Similar schools average";
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";
}

public record GradeOptionViewModel(string Key, string Name);
