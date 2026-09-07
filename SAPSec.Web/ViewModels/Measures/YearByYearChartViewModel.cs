namespace SAPSec.Web.ViewModels.Measures;

public record YearByYearChartViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<YearByYearSeriesViewModel> Series,
    IEnumerable<string> Colors)
    : MeasureBreakdownViewModel(MeasureInfo);

public record YearByYearSeriesViewModel(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);