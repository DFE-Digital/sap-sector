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
           MeasureDataType.DestinationsPercentage => Display1DecimalPlacePercent(value),
           MeasureDataType.OverallAbsencePercentage or MeasureDataType.PersistentAbsencePercentage => Display2DecimalPlacesPercent(value),
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

    public static string Display1DecimalPlacePercent(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%"
            : "No available data";

    public static string Display2DecimalPlacesPercent(decimal? value) =>
         value.HasValue
            ? value.Value.ToString("0.00", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
 