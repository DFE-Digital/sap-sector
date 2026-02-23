using System.Globalization;
using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class SimilarSchoolsComparisonViewModel
{
    public required string Urn { get; set; }
    public required string SimilarSchoolUrn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolName { get; set; }
    public double? CurrentSchoolLatitude { get; set; }
    public double? CurrentSchoolLongitude { get; set; }
    public double? SimilarSchoolLatitude { get; set; }
    public double? SimilarSchoolLongitude { get; set; }
    public double? Distance { get; set; }
    public string DistanceDisplay =>
        Distance is double d
            ? d.ToString("0.00", CultureInfo.InvariantCulture)
            : string.Empty;
    public SchoolDetails? SimilarSchoolDetails { get; set; }
}