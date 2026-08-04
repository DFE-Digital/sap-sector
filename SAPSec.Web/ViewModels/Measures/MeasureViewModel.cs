using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.ViewModels.Measures;

public record MeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    CurrentYearChartViewModel CurrentYear,
    YearByYearChartViewModel YearByYear,
    TableViewModel Table,
    TopPerformersViewModel? TopPerformers)
{
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

    public static MeasureViewModel FromMeasure(
        Measure measure,
        SchoolInfo schoolInfo,
        SchoolInfo? similarSchool,
        Func<string, string> viewSimilarSchoolsUrl,
        Func<string, string, string> similarSchoolComparisonUrl)
    {
        var measureInfo = new MeasureInfoViewModel(
            measure.Key,
            measure.Name,
            measure.Year,
            measure.DataType,
            measure.Filters.Select(MapAvailableFilter),
            measure.Series.Select(s => ResolveSeriesLabel(s.SeriesType, schoolInfo, similarSchool)),
            null,
            null);

        decimal? MapCurrentYear(MeasureSeries series) =>
            series.Current;

        var currentYear = new CurrentYearChartViewModel(
            measureInfo,
            measure.Series.Select(MapCurrentYear));

        YearByYearSeriesViewModel MapYearByYear(MeasureSeries series) =>
            new(series.Current, series.Previous, series.Previous2);

        var yearByYear = new YearByYearChartViewModel(
            measureInfo,
            measure.Series.Select(MapYearByYear));

        TableRowViewModel MapTableRow(MeasureSeries series) =>
            new(MapYearByYear(series), MapCurrentYear(series));

        var table = new TableViewModel(
            measureInfo,
            measure.Series.Select(MapTableRow));

        TopPerformerViewModel MapTopPerformer(TopPerformer t) => new TopPerformerViewModel(
            t.Rank,
            t.Urn,
            t.Name,
            similarSchoolComparisonUrl(schoolInfo.Urn, t.Urn),
            t.Value,
            t.IsCurrentSchool);

        TopPerformersViewModel? topPerformers = null;
        if (measure.TopPerformers is not null)
        {
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
    IEnumerable<string>? ChartColors = null,
    IEnumerable<string>? YearByYearColors = null);

public record MeasureAvailableFilterViewModel(
    string Key,
    string Name,
    IEnumerable<MeasureFilterOptionViewModel> Options);

public record MeasureFilterOptionViewModel(
    string Key,
    string Name,
    int Count,
    bool Selected);
