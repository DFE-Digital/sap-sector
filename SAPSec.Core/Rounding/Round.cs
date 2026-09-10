namespace SAPSec.Core.Rounding;

internal static class Round
{
    public static int ToInt(decimal value) =>
        Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));

    public static decimal ToWholeNumber(decimal value) =>
        decimal.Round(value, 0, MidpointRounding.AwayFromZero);

    public static decimal ToOneDecimalPlace(decimal value) =>
        decimal.Round(value, 1, MidpointRounding.AwayFromZero);

    public static decimal ToThreeDecimalPlaces(decimal value) =>
        decimal.Round(value, 3, MidpointRounding.AwayFromZero);
}
