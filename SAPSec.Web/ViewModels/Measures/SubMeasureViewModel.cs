using SAPSec.Core.Measures;
using SAPSec.Core.SchoolDetails;
using SAPSec.Web.Constants;
using System.Globalization;

namespace SAPSec.Web.ViewModels.Measures;

public abstract record SubMeasureViewModel(
    MeasureInfoViewModel MeasureInfo)
{
    public abstract string Id { get; }
    public abstract string Name { get; }

    public string DisplayNumber(decimal? value) =>
        MeasureInfo.DataType == MeasureDataType.Score
            ? DisplayValue(value)
            : DisplayWholePercent(value);

    private static string DisplayValue(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.0", CultureInfo.InvariantCulture)
            : "No available data";

    private static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";

    public static SubMeasureViewModel FromSubMeasure(SubMeasure m, MeasureInfoViewModel measureInfo, SchoolInfo schoolInfo)
    {
        return m switch
        {
            ThreeYearAverageSubMeasure tya => new ThreeYearAverageSubMeasureViewModel(
                measureInfo,
                tya.Averages),

            YearByYearSubMeasure yby => new YearByYearSubMeasureViewModel(
                measureInfo,
                yby.Series.Select(MapYearByYear)),

            TopPerformersSubMeasure tp => new TopPerformersSubMeasureViewModel(
                measureInfo,
                tp.TopPerformers.Select(t => new TopPerformersSubMeasureItemViewModel(
                    t.Rank,
                    t.Urn,
                    t.Name,
                    Routes.SimilarSchoolComparison(schoolInfo.Urn, t.Urn),
                    t.Value,
                    t.IsCurrentSchool)),
                Routes.SimilarSchools(schoolInfo.Urn)),

            _ => throw new NotImplementedException($"SubMeasure type {m.GetType()} does not have a corresponding view model type associated.")
        };
    }

    private static YearByYearSeriesViewModel MapYearByYear(YearByYearSeries yearByYear) =>
        new(yearByYear.Current, yearByYear.Previous, yearByYear.Previous2);
}
