using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public record DemoSimilarSchoolsComparisonViewModel
{
    public required string Name { get; set; }
    public required string SimilarSchoolName { get; set; }
    public required double CurrentSchoolLatitude { get; set; }
    public required double CurrentSchoolLongitude { get; set; }
    public required double SimilarSchoolLatitude { get; set; }
    public required double SimilarSchoolLongitude { get; set; }
    public required double Distance { get; set; }
    public required SchoolDetails SimilarSchoolDetails { get; set; }
}