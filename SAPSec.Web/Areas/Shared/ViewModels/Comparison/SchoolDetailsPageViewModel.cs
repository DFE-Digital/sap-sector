using System.Globalization;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public class SchoolDetailsPageViewModel
{
    public required SchoolViewModel CurrentSchool { get; set; }
    public required SchoolViewModel ComparatorSchool { get; set; }

    public double? CurrentSchoolLatitude { get; init; }
    public double? CurrentSchoolLongitude { get; init; }
    public double? ComparatorSchoolLatitude { get; init; }
    public double? ComparatorSchoolLongitude { get; init; }
    public double? Distance { get; init; }
    public required SchoolDetailsViewModel ComparatorSchoolDetails { get; init; }

    public string DistanceDisplay =>
        Distance is double d
            ? d.ToString("0.0", CultureInfo.InvariantCulture)
            : string.Empty;
}
