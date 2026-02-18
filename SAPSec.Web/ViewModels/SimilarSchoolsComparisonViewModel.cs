using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class SimilarSchoolsComparisonViewModel
{
    public string Urn { get; set; }
    public string SimilarSchoolUrn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolName { get; set; }
    public required double CurrentSchoolLatitude { get; set; }
    public required double CurrentSchoolLongitude { get; set; }
    public required double SimilarSchoolLatitude { get; set; }
    public required double SimilarSchoolLongitude { get; set; }
    public required double Distance { get; set; }
    public required SchoolDetails SimilarSchoolDetails { get; set; }
}