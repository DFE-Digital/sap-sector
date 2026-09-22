using SAPSec.Core.Features.Measures;
using System.Globalization;

namespace SAPSec.Web.ViewModels.Measures;

public abstract record MeasureBreakdownViewModel(
    MeasureInfoViewModel MeasureInfo)
{
    public string DisplayNumber(decimal? value) =>
       MeasureInfo.DataType switch
       {
           MeasureDataType.Score or MeasureDataType.ScaledScore => DisplayValue(value),
           MeasureDataType.DestinationsPercentage => DisplayPercent(value, 1),
           MeasureDataType.OverallAbsencePercentage or MeasureDataType.PersistentAbsencePercentage => DisplayPercent(value, 2),
           _ => DisplayWholePercent(value)
       };           

    private static string DisplayValue(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.0", CultureInfo.InvariantCulture)
            : "No available data";

    private static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";

    public static string DisplayPercent(decimal? value, int decimalPlaces) =>
    value.HasValue
        ? value.Value.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture) + "%"
        : "No available data";
}
 