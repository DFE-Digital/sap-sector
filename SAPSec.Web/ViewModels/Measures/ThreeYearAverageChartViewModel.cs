namespace SAPSec.Web.ViewModels.Measures;

public record ThreeYearAverageChartViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<decimal?> Averages)
    : MeasureBreakdownViewModel(MeasureInfo);
