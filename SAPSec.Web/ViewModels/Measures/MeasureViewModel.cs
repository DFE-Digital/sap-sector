using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.Constants;

namespace SAPSec.Web.ViewModels.Measures;

public record MeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    CurrentYearChartViewModel CurrentYear,
    YearByYearChartViewModel YearByYear,
    TableViewModel Table,
    TopPerformersViewModel? TopPerformers)
{
    // TODO: Colours should be a CSS concern, this should instead be implemented as a class on the chart element
    // which sets the appropriate CSS variables --chart-color-school etc. (see chart-factory.js)
    private static readonly string[] SchoolCurrentYearColors = ["#ca357c", "#2a1950", "#2a1950", "#2a1950"];
    private static readonly string[] SchoolYearByYearColors = ["#ca357c", "#2a1950", "#5694ca", "#4b9b7d"];
    // chart-factory.js assigns bar/line colours to datasets by array position (school/similarSchools/
    // localAuthority/england), which assumes the 4-series shape produced by Measure.ForSchool. The 3-series
    // comparison shape (CurrentSchool/SimilarSchool/EnglandSchoolsAverage) puts England at the position
    // chart-factory.js treats as "local authority", so comparison charts must supply explicit colours.
    private static readonly string[] ComparisonCurrentYearColors = ["#ca357c", "#2a1950", "#2a1950"];
    private static readonly string[] ComparisonYearByYearColors = ["#ca357c", "#2a1950", "#4b9b7d"];

    public static MeasureViewModel FromPrimaryMeasure(Measure measure, SchoolInfo schoolInfo)
        => FromMeasure(measure, schoolInfo, null,
            urn => Routes.PrimarySchool(urn).ViewSimilarSchools,
            (currentSchoolUrn, similarSchoolUrn) => Routes.PrimarySchool(currentSchoolUrn).Comparison(similarSchoolUrn).Similarity,
            SchoolCurrentYearColors,
            SchoolYearByYearColors);

    public static MeasureViewModel FromPrimaryComparisonMeasure(Measure measure, SchoolInfo schoolInfo, SchoolInfo similarSchool)
        => FromMeasure(measure, schoolInfo, similarSchool,
            urn => Routes.PrimarySchool(urn).ViewSimilarSchools,
            (currentSchoolUrn, similarSchoolUrn) => Routes.PrimarySchool(currentSchoolUrn).Comparison(similarSchoolUrn).Similarity,
            ComparisonCurrentYearColors,
            ComparisonYearByYearColors);

    public static MeasureViewModel FromSecondaryMeasure(Measure measure, SchoolInfo schoolInfo)
        => FromMeasure(measure, schoolInfo, null,
            urn => Routes.SecondarySchool(urn).ViewSimilarSchools,
            (currentSchoolUrn, similarSchoolUrn) => Routes.SecondarySchool(currentSchoolUrn).Comparison(similarSchoolUrn).Similarity,
            SchoolCurrentYearColors,
            SchoolYearByYearColors);

    public static MeasureViewModel FromSecondaryComparisonMeasure(Measure measure, SchoolInfo schoolInfo, SchoolInfo similarSchool)
        => FromMeasure(measure, schoolInfo, similarSchool,
            urn => Routes.SecondarySchool(urn).ViewSimilarSchools,
            (currentSchoolUrn, similarSchoolUrn) => Routes.SecondarySchool(currentSchoolUrn).Comparison(similarSchoolUrn).Similarity,
            ComparisonCurrentYearColors,
            ComparisonYearByYearColors);

    private static MeasureViewModel FromMeasure(
        Measure measure,
        SchoolInfo schoolInfo,
        SchoolInfo? similarSchool,
        Func<string, string> viewSimilarSchoolsUrl,
        Func<string, string, string> similarSchoolComparisonUrl,
        string[] currentYearChartColors,
        string[] yearByYearChartColors)
    {
        var measureInfo = new MeasureInfoViewModel(
            measure.Key,
            measure.Name,
            measure.Year,
            measure.DataType,
            measure.Filters.Select(MapAvailableFilter),
            measure.Series.Select(s => ResolveSeriesLabel(s.SeriesType, schoolInfo, similarSchool)),
            measure.Series.Select(s => ResolveSeriesPointStyle(s.SeriesType)));

        decimal? MapCurrentYear(MeasureSeries series) =>
            series.Current;

        var currentYear = new CurrentYearChartViewModel(
            measureInfo,
            measure.Series.Select(MapCurrentYear),
            currentYearChartColors);

        YearByYearSeriesViewModel MapYearByYear(MeasureSeries series) =>
            new(series.Current, series.Previous, series.Previous2);

        var yearByYear = new YearByYearChartViewModel(
            measureInfo,
            measure.Series.Select(MapYearByYear),
            yearByYearChartColors);

        TableRowViewModel MapTableRow(MeasureSeries series) =>
            new(MapYearByYear(series), MapCurrentYear(series));

        var table = new TableViewModel(
            measureInfo,
            measure.Series.Select(MapTableRow));

        TopPerformersViewModel? topPerformers = null;

        if (measure.TopPerformers is not null)
        {
            TopPerformerViewModel MapTopPerformer(TopPerformer t) => new TopPerformerViewModel(
                t.Rank,
                t.Urn,
                t.Name,
                similarSchoolComparisonUrl(schoolInfo.Urn, t.Urn),
                t.Value,
                t.IsCurrentSchool);

            topPerformers = new TopPerformersViewModel(
                measureInfo,
                measure.TopPerformers.Select(MapTopPerformer),
                viewSimilarSchoolsUrl(schoolInfo.Urn));
        }

        return new(
            measureInfo,
            currentYear,
            yearByYear,
            table,
            topPerformers);
    }

    private static string ResolveSeriesLabel(MeasureSeriesType seriesType, SchoolInfo currentSchool, SchoolInfo? similarSchool = null) =>
       seriesType switch
       {
           MeasureSeriesType.CurrentSchool => currentSchool.Name,
           MeasureSeriesType.SimilarSchool => similarSchool?.Name ??
               throw new InvalidOperationException($"Similar school required to resolve label for Measure Series Type: {Enum.GetName(seriesType)}"),
           MeasureSeriesType.SimilarSchoolsAverage => "Similar schools average",
           MeasureSeriesType.LASchoolsAverage => "Local authority schools average",
           MeasureSeriesType.EnglandSchoolsAverage => "Schools in England average",
           _ => throw new InvalidOperationException($"No label found for Measure Series Type: {Enum.GetName(seriesType)}")
       };

    private static string ResolveSeriesPointStyle(MeasureSeriesType seriesType) =>
        seriesType switch
        {
            MeasureSeriesType.CurrentSchool => "triangle",
            MeasureSeriesType.SimilarSchool => "circle",
            MeasureSeriesType.SimilarSchoolsAverage => "circle",
            MeasureSeriesType.LASchoolsAverage => "rect",
            MeasureSeriesType.EnglandSchoolsAverage => "rectRot",
            _ => "circle"
        };

    private static MeasureAvailableFilterViewModel MapAvailableFilter(MeasureAvailableFilter availableFilter) =>
        new(availableFilter.Key, availableFilter.Name, availableFilter.Options.Select(o => new MeasureFilterOptionViewModel(o.Key, o.Name, o.Count, o.Selected)));
}

public record MeasureInfoViewModel(
    string HtmlPrefix,
    string Name,
    int Year,
    MeasureDataType DataType,
    IEnumerable<MeasureAvailableFilterViewModel> Filters,
    IEnumerable<string> Labels,
    IEnumerable<string> YearByYearPointStyles);

public record MeasureAvailableFilterViewModel(
    string Key,
    string Name,
    IEnumerable<MeasureFilterOptionViewModel> Options);

public record MeasureFilterOptionViewModel(
    string Key,
    string Name,
    int Count,
    bool Selected);
