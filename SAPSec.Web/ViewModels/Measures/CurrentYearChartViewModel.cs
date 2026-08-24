namespace SAPSec.Web.ViewModels.Measures;

public record CurrentYearChartViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<decimal?> Averages,
    IEnumerable<string> Colors)
    : MeasureBreakdownViewModel(MeasureInfo);
