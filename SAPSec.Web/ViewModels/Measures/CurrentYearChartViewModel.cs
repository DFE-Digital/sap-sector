namespace SAPSec.Web.ViewModels.Measures;

public record CurrentYearChartViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<decimal?> Averages)
    : MeasureBreakdownViewModel(MeasureInfo);
