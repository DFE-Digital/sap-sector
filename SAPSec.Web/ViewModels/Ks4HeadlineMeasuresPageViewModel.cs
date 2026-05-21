using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Model;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public record TopPerformerRow(int Rank, string Urn, string Name, decimal? Value, string DisplayValue, bool IsCurrentSchool);

public record SubjectSection(
    decimal? SchoolThreeYearAverage,
    decimal? SimilarSchoolsThreeYearAverage,
    decimal? LocalAuthorityThreeYearAverage,
    decimal? EnglandThreeYearAverage,
    IReadOnlyList<TopPerformerRow> TopPerformers,
    Ks4HeadlineMeasureSeries SchoolYearByYear,
    Ks4HeadlineMeasureSeries SimilarSchoolsYearByYear,
    Ks4HeadlineMeasureSeries LocalAuthorityYearByYear,
    Ks4HeadlineMeasureSeries EnglandYearByYear)
{
    public string SchoolDisplay => DisplayWholePercent(SchoolThreeYearAverage);
    public string SimilarSchoolsDisplay => DisplayWholePercent(SimilarSchoolsThreeYearAverage);
    public string LocalAuthorityDisplay => DisplayWholePercent(LocalAuthorityThreeYearAverage);
    public string EnglandDisplay => DisplayWholePercent(EnglandThreeYearAverage);

    public static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";

    public static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}

public class Ks4HeadlineMeasuresPageViewModel
{
    public required SchoolDetails SchoolDetails { get; set; }
    public required int SimilarSchoolsCount { get; set; }

    public required SubjectSection Attainment8 { get; set; }
    public required SubjectSection EngMaths { get; set; }
    public required SubjectSection Destinations { get; set; }

    public string SchoolLabel => SchoolDetails.Name;
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
