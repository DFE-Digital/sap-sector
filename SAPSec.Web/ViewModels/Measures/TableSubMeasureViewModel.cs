namespace SAPSec.Web.ViewModels.Measures;

public record TableSubMeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<TableSubMeasureRowViewModel> Rows) : SubMeasureViewModel(MeasureInfo)
{
    public override string Id => "table-view";
    public override string Name => "Table";

    public static SubMeasureViewModel FromOtherSubMeasures(IEnumerable<SubMeasureViewModel> subMeasures, MeasureInfoViewModel measureInfo)
    {
        var yearByYear = subMeasures.OfType<YearByYearSubMeasureViewModel>().FirstOrDefault();
        var threeYearAverage = subMeasures.OfType<ThreeYearAverageSubMeasureViewModel>().FirstOrDefault();

        return new TableSubMeasureViewModel(
            measureInfo,
            (yearByYear?.Series ?? []).Zip(threeYearAverage?.Averages ?? [])
                .Select(x => new TableSubMeasureRowViewModel(x.First, x.Second)));
    }
}

public record TableSubMeasureRowViewModel(
    YearByYearSeriesViewModel YearByYear,
    decimal? ThreeYearAverage);