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
           MeasureDataType.DestinationPercentage => DisplayPercent(value, 1),
           MeasureDataType.OverallAbsencePercentage or MeasureDataType.PersistentAbsencePercentage => DisplayPercent(value, 2),
           _ => DisplayPercent(value, 0)
       };

    private static string DisplayValue(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.0", CultureInfo.InvariantCulture)
            : "No available data";

    public static string DisplayPercent(decimal? value, int decimalPlaces = 2) =>
        value.HasValue
            ? Math.Round(value.Value, decimalPlaces, MidpointRounding.AwayFromZero)
                .ToString(decimalPlaces == 0 ? "0" : $"0.{new string('0', decimalPlaces)}", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
 