using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public class AttendancePageViewModel
{
    public required string Urn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolUrn { get; set; }
    public required string SimilarSchoolName { get; set; }

    public required MeasureViewModel Absence { get; set; }
}
