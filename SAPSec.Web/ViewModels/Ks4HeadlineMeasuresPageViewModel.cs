using SAPSec.Web.ViewModels.Measures;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public record TopPerformerRow(int Rank, string Urn, string Name, decimal? Value, string DisplayValue, bool IsCurrentSchool);

public class Ks4HeadlineMeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required int SimilarSchoolsCount { get; set; }

    public required MeasureViewModel Attainment8 { get; set; }
    public required MeasureViewModel EnglishAndMaths { get; set; }
    public required MeasureViewModel Destinations { get; set; }

    public string SchoolLabel => School.Name;
    public string SimilarSchoolsLabel => $"Similar schools average";
    public string LocalAuthorityLabel => $"Local authority schools average";
    public string EnglandLabel => "Schools in England average";

    public static string DisplayValue(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) : "No available data";

    public static string DisplayWholeValue(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture)
            : "No available data";

    public static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";

    public static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
