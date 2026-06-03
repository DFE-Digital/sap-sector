namespace SAPSec.Web.ViewModels.Measures;

public record ThreeYearAverageSubMeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<decimal?> Averages) : SubMeasureViewModel(MeasureInfo)
{
    public override string Id => "three-year-average";
    public override string Name => "3-year average";
}
