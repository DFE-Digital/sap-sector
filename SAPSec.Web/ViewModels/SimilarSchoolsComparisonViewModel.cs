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
    }

    public decimal? ThisSchoolAttainment8ThreeYearAverage { get; set; }
    public decimal? SelectedSchoolAttainment8ThreeYearAverage { get; set; }
    public decimal? EnglandAttainment8ThreeYearAverage { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? ThisSchoolAttainment8YearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? SelectedSchoolAttainment8YearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? EnglandAttainment8YearByYear { get; set; }
    public int? ThisSchoolTotalPupils { get; set; }
    public int? SelectedSchoolTotalPupils { get; set; }
    public int? EnglandTotalPupils { get; set; }

    public string ThisSchoolAttainment8Display => DisplayValue(ThisSchoolAttainment8ThreeYearAverage);
    public string SelectedSchoolAttainment8Display => DisplayValue(SelectedSchoolAttainment8ThreeYearAverage);
    public string EnglandAttainment8Display => DisplayValue(EnglandAttainment8ThreeYearAverage);
    public string ThisSchoolTotalPupilsDisplay => DisplayPupilCount(ThisSchoolTotalPupils);
    public string SelectedSchoolTotalPupilsDisplay => DisplayPupilCount(SelectedSchoolTotalPupils);
    public string EnglandTotalPupilsDisplay => DisplayPupilCount(EnglandTotalPupils);

    public decimal ThisSchoolWidthPercent => WidthPercent(ThisSchoolAttainment8ThreeYearAverage);
    public decimal SelectedSchoolWidthPercent => WidthPercent(SelectedSchoolAttainment8ThreeYearAverage);
    public decimal EnglandWidthPercent => WidthPercent(EnglandAttainment8ThreeYearAverage);

    private const decimal Attainment8MaxScore = 90m;

    private decimal WidthPercent(decimal? value)
    {
        if (!value.HasValue)
        {
            return 0m;
        }

        var clamped = Math.Clamp(value.Value, 0m, Attainment8MaxScore);
        return Math.Round((clamped / Attainment8MaxScore) * 100m, 1, MidpointRounding.AwayFromZero);
    }

    private static string DisplayValue(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) : "No available data";

    private static string DisplayPupilCount(int? value) =>
        value.HasValue ? value.Value.ToString("N0", CultureInfo.InvariantCulture) : "No available data";
}
