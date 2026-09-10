using System.Globalization;
using SAPSec.Core.Features.SchoolDetails;

namespace SAPSec.Web.ViewModels;

public class SimilarSchoolDetailsViewModel
{
    public required string CurrentSchoolUrn { get; init; }
    public required string ComparatorSchoolUrn { get; init; }
    public required string CurrentSchoolName { get; init; }
    public required string ComparatorSchoolName { get; init; }
    public double? CurrentSchoolLatitude { get; init; }
    public double? CurrentSchoolLongitude { get; init; }
    public double? ComparatorSchoolLatitude { get; init; }
    public double? ComparatorSchoolLongitude { get; init; }
    public double? Distance { get; init; }
    public SchoolDetails? ComparatorSchoolDetails { get; init; }

    public string DistanceDisplay =>
        Distance is double d
            ? d.ToString("0.0", CultureInfo.InvariantCulture)
            : string.Empty;
}
