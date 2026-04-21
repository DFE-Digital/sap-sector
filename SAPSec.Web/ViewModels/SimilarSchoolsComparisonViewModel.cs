using System.Globalization;
using SAPSec.Core.Model;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

namespace SAPSec.Web.ViewModels;

public class SimilarSchoolsComparisonViewModel
{
    public record CoreSubjectSection(
        decimal? ThisSchoolThreeYearAverage,
        decimal? SimilarSchoolThreeYearAverage,
        decimal? EnglandThreeYearAverage,
        Ks4HeadlineMeasureSeries? ThisSchoolYearByYear,
        Ks4HeadlineMeasureSeries? SimilarSchoolYearByYear,
        Ks4HeadlineMeasureSeries? EnglandYearByYear)
    {
        public string ThisSchoolDisplay => DisplayWholePercent(ThisSchoolThreeYearAverage);
        public string SimilarSchoolDisplay => DisplayWholePercent(SimilarSchoolThreeYearAverage);
        public string EnglandDisplay => DisplayWholePercent(EnglandThreeYearAverage);
    }

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
    public decimal? ThisSchoolAttainment8ThreeYearAverage { get; set; }
    public decimal? SelectedSchoolAttainment8ThreeYearAverage { get; set; }
    public decimal? EnglandAttainment8ThreeYearAverage { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? ThisSchoolAttainment8YearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? SelectedSchoolAttainment8YearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? EnglandAttainment8YearByYear { get; set; }
    public decimal? ThisSchoolEngMaths49ThreeYearAverage { get; set; }
    public decimal? SelectedSchoolEngMaths49ThreeYearAverage { get; set; }
    public decimal? EnglandEngMaths49ThreeYearAverage { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? ThisSchoolEngMaths49YearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? SelectedSchoolEngMaths49YearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? EnglandEngMaths49YearByYear { get; set; }
    public decimal? ThisSchoolEngMaths59ThreeYearAverage { get; set; }
    public decimal? SelectedSchoolEngMaths59ThreeYearAverage { get; set; }
    public decimal? EnglandEngMaths59ThreeYearAverage { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? ThisSchoolEngMaths59YearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? SelectedSchoolEngMaths59YearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? EnglandEngMaths59YearByYear { get; set; }
    public decimal? ThisSchoolDestinationsThreeYearAverage { get; set; }
    public decimal? SelectedSchoolDestinationsThreeYearAverage { get; set; }
    public decimal? EnglandDestinationsThreeYearAverage { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? ThisSchoolDestinationsYearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? SelectedSchoolDestinationsYearByYear { get; set; }
    public SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries? EnglandDestinationsYearByYear { get; set; }
    public CoreSubjectSection? EnglishLanguage { get; set; }
    public CoreSubjectSection? EnglishLiterature { get; set; }
    public CoreSubjectSection? Biology { get; set; }
    public CoreSubjectSection? Chemistry { get; set; }
    public CoreSubjectSection? Physics { get; set; }
    public CoreSubjectSection? Maths { get; set; }
    public CoreSubjectSection? CombinedScienceDoubleAward { get; set; }

    public string ThisSchoolAttainment8Display => DisplayValue(ThisSchoolAttainment8ThreeYearAverage);
    public string SelectedSchoolAttainment8Display => DisplayValue(SelectedSchoolAttainment8ThreeYearAverage);
    public string EnglandAttainment8Display => DisplayValue(EnglandAttainment8ThreeYearAverage);
    public string ThisSchoolEngMaths49Display => DisplayPercent(ThisSchoolEngMaths49ThreeYearAverage);
    public string SelectedSchoolEngMaths49Display => DisplayPercent(SelectedSchoolEngMaths49ThreeYearAverage);
    public string EnglandEngMaths49Display => DisplayPercent(EnglandEngMaths49ThreeYearAverage);
    public string ThisSchoolEngMaths59Display => DisplayPercent(ThisSchoolEngMaths59ThreeYearAverage);
    public string SelectedSchoolEngMaths59Display => DisplayPercent(SelectedSchoolEngMaths59ThreeYearAverage);
    public string EnglandEngMaths59Display => DisplayPercent(EnglandEngMaths59ThreeYearAverage);
    public string ThisSchoolDestinationsDisplay => DisplayWholePercent(ThisSchoolDestinationsThreeYearAverage);
    public string SelectedSchoolDestinationsDisplay => DisplayWholePercent(SelectedSchoolDestinationsThreeYearAverage);
    public string EnglandDestinationsDisplay => DisplayWholePercent(EnglandDestinationsThreeYearAverage);

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

    public static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";

    public static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
