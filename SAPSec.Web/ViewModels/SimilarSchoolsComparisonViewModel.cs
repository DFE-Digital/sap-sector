using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;
using SAPSec.Web.ViewModels.Measures;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public class SimilarSchoolsComparisonViewModel
{
    public required string Urn { get; set; }
    public required string SimilarSchoolUrn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolName { get; set; }

    // ----------------------------
    // School Details
    // ----------------------------
    public double? CurrentSchoolLatitude { get; set; }
    public double? CurrentSchoolLongitude { get; set; }
    public double? SimilarSchoolLatitude { get; set; }
    public double? SimilarSchoolLongitude { get; set; }
    public double? Distance { get; set; }

    public string DistanceDisplay =>
        Distance is double d
            ? d.ToString("0.0", CultureInfo.InvariantCulture)
            : string.Empty;

    public SchoolDetails? SimilarSchoolDetails { get; set; }

    // ----------------------------
    // Similarity (9 characteristics table)
    // ----------------------------
    public IReadOnlyList<CharacteristicRow> CharacteristicsRows { get; set; }
        = Array.Empty<CharacteristicRow>();

    public sealed class CharacteristicRow
    {
        public required string Characteristic { get; init; }
        public required string CurrentSchoolValue { get; init; }
        public required string SimilarSchoolValue { get; init; }
        public bool IsNumeric { get; init; }

        public SchoolSimilarity Similarity { get; init; }
    }

    public MeasureViewModel? Attainment8 { get; set; }
    public MeasureViewModel? EnglishAndMaths { get; set; }
    public MeasureViewModel? Destinations { get; set; }

    public IEnumerable<MeasureViewModel>? Measures { get; set; }

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
