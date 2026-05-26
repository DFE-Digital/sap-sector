using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public class MeasureViewModel
{
    public required string SchoolUrn { get; set; }
    public required string SchoolName { get; set; }
    public required string HtmlPrefix { get; set; }
    public required MeasureDataType DataType { get; set; }
    public required decimal? SchoolThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityThreeYearAverage { get; set; }
    public required decimal? EnglandThreeYearAverage { get; set; }
    public required IReadOnlyList<TopPerformerRow> TopPerformers { get; set; }
    public required Ks4HeadlineMeasureSeries SchoolYearByYear { get; set; }
    public required Ks4HeadlineMeasureSeries SimilarSchoolsYearByYear { get; set; }
    public required Ks4HeadlineMeasureSeries LocalAuthorityYearByYear { get; set; }
    public required Ks4HeadlineMeasureSeries EnglandYearByYear { get; set; }

    public string SimilarSchoolsLabel => "Similar schools average";
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";

    public string DisplayNumber(decimal? value) =>
        this.DataType == MeasureDataType.Number
            ? DisplayValue(value)
            : DisplayWholePercent(value);

    private static string DisplayValue(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) : "No available data";

    private static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";

    public string SchoolDisplay => DisplayWholePercent(SchoolThreeYearAverage);
    public string SimilarSchoolsDisplay => DisplayWholePercent(SimilarSchoolsThreeYearAverage);
    public string LocalAuthorityDisplay => DisplayWholePercent(LocalAuthorityThreeYearAverage);
    public string EnglandDisplay => DisplayWholePercent(EnglandThreeYearAverage);
}

public record GradeOptionViewModel(string Key, string Name);
