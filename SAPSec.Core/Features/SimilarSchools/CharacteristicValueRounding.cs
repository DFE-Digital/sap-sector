namespace SAPSec.Core.Features.SimilarSchools;

internal static class CharacteristicValueRounding
{
    public static int RoundInt(decimal value) =>
        Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));

    public static decimal RoundWholeNumber(decimal value) =>
        decimal.Round(value, 0, MidpointRounding.AwayFromZero);

    public static decimal RoundToOneDecimalPlace(decimal value) =>
        decimal.Round(value, 1, MidpointRounding.AwayFromZero);

    public static decimal RoundToThreeDecimalPlaces(decimal value) =>
        decimal.Round(value, 3, MidpointRounding.AwayFromZero);
}
