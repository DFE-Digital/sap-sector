using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.Constants;
using static SAPSec.Core.Constants.Measures.Primary;

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

    private static string ResolveMeasureLabel(string measureKey) =>
        measureKey switch
        {
            Ks2ExpectedRwm.Key =>
                "Meeting expected standard in reading, writing and maths",
            _ => throw new InvalidOperationException($"No label found for Measure Key: {measureKey}")
        };

    public static MeasureViewModel FromMeasure(
        Measure measure,
        SchoolInfo schoolInfo,
        IEnumerable<string>? chartColors = null,
        IEnumerable<string>? yearByYearColors = null)
    {
        var measureInfo = new MeasureInfoViewModel(
            measure.Key,
            ResolveMeasureLabel(measure.Key),
            measure.DataType,
            measure.Filters.Select(MapAvailableFilter),
            measure.Series.Select(s => ResolveSeriesLabel(s.SeriesType, schoolInfo)),
            chartColors,
            yearByYearColors);

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
            Routes.PrimarySchool(schoolInfo.Urn).SimilarSchoolComparison(t.Urn),
            t.Value,
            t.IsCurrentSchool);

        TopPerformersViewModel? topPerformers = null;
        if (measure.TopPerformers is not null)
        {
            topPerformers = new TopPerformersViewModel(
                measureInfo,
                measure.TopPerformers.Select(MapTopPerformer),
                Routes.PrimarySchool(schoolInfo.Urn).ViewSimilarSchools);
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