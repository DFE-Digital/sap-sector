using SAPSec.Core.Model;
using SAPSec.Web.ViewModels.Measures;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public class Ks4CoreSubjectsPageViewModel
{
    public required SchoolDetails SchoolDetails { get; set; }
    public required int SimilarSchoolsCount { get; set; }
    public required IEnumerable<MeasureViewModel> Measures { get; set; }

    public string SchoolLabel => SchoolDetails.Name;
    public string SimilarSchoolsLabel => "Similar schools average";
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";

    public static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";

    public static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
