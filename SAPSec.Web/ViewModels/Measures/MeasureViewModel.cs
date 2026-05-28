using SAPSec.Core.Features.Measures;
using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels.Measures;

public record MeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<SubMeasureViewModel> SubMeasures)
{
    public static MeasureViewModel FromMeasure(
        Measure measure,
        SchoolDetails schoolDetails,
        IEnumerable<string> labels,
        IEnumerable<string>? chartColors = null,
        IEnumerable<string>? yearByYearColors = null)
    {
        var measureInfo = new MeasureInfoViewModel(
            measure.Key,
            measure.Name,
            measure.DataType,
            measure.Filters.Select(MapAvailableFilter),
            labels,
            chartColors,
            yearByYearColors);

        var subMeasures = measure.SubMeasures.Select(m => SubMeasureViewModel.FromSubMeasure(m, measureInfo, schoolDetails));
        var table = TableSubMeasureViewModel.FromOtherSubMeasures(subMeasures, measureInfo);

        return new(
            measureInfo,
            subMeasures
                .Append(table)
                .OrderBy(x => x switch
                {
                    ThreeYearAverageSubMeasureViewModel => 0,
                    YearByYearSubMeasureViewModel => 1,
                    TableSubMeasureViewModel => 2,
                    TopPerformersSubMeasureViewModel => 3,
                    _ => 9
                }));
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