namespace SAPSec.Web.ViewModels.Measures;

public record YearByYearSubMeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<YearByYearSeriesViewModel> Series) : SubMeasureViewModel(MeasureInfo)
{
    public override string Id => "year-by-year";
    public override string Name => "Year by year";
}

public record YearByYearSeriesViewModel(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);