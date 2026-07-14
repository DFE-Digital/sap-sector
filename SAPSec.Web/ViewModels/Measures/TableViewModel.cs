namespace SAPSec.Web.ViewModels.Measures;

public record TableViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<TableRowViewModel> Rows)
    : MeasureBreakdownViewModel(MeasureInfo);

public record TableRowViewModel(
    YearByYearSeriesViewModel YearByYear,
    decimal? ThreeYearAverage);