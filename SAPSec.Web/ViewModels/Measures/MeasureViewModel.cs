using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.Constants;

namespace SAPSec.Web.ViewModels.Measures;

public record MeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    ThreeYearAverageChartViewModel ThreeYearAverage,
    YearByYearChartViewModel YearByYear,
    TableViewModel Table,
    TopPerformersViewModel? TopPerformers)
{
    public static MeasureViewModel FromMeasure(
        Measure measure,
        SchoolInfo schoolInfo,
        IEnumerable<string>? chartColors = null,
        IEnumerable<string>? yearByYearColors = null)
    {
        var measureInfo = new MeasureInfoViewModel(
            measure.Key,
            measure.Name,
            measure.DataType,
            measure.Filters.Select(MapAvailableFilter),
            measure.Series.Select(s => s.Label),
            chartColors,
            yearByYearColors);

        decimal? MapThreeYearAverage(MeasureSeries series) =>
            series.ThreeYearAverage;

        var threeYearAverage = new ThreeYearAverageChartViewModel(
            measureInfo,
            measure.Series.Select(MapThreeYearAverage));

        YearByYearSeriesViewModel MapYearByYear(MeasureSeries series) =>
            new(series.YearByYear.Current, series.YearByYear.Previous, series.YearByYear.Previous2);

        var yearByYear = new YearByYearChartViewModel(
            measureInfo,
            measure.Series.Select(MapYearByYear));

        TableRowViewModel MapTableRow(MeasureSeries series) =>
            new(MapYearByYear(series), MapThreeYearAverage(series));

        var table = new TableViewModel(
            measureInfo,
            measure.Series.Select(MapTableRow));

        TopPerformerViewModel MapTopPerformer(TopPerformer t) => new TopPerformerViewModel(
            t.Rank,
            t.Urn,
            t.Name,
            Routes.Primary.SimilarSchoolComparison(schoolInfo.Urn, t.Urn),
            t.Value,
            t.IsCurrentSchool);

        TopPerformersViewModel? topPerformers = null;
        if (measure.TopPerformers is not null)
        {
            topPerformers = new TopPerformersViewModel(
                measureInfo,
                measure.TopPerformers.Select(MapTopPerformer),
                Routes.Primary.SimilarSchools(schoolInfo.Urn));
        }

        return new(
            measureInfo,
            threeYearAverage,
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