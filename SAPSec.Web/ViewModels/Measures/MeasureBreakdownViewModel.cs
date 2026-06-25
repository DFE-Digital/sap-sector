using SAPSec.Core.Features.Measures;
using System.Globalization;

namespace SAPSec.Web.ViewModels.Measures;

public abstract record MeasureBreakdownViewModel(
    MeasureInfoViewModel MeasureInfo)
{
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
}
