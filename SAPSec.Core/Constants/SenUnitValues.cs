namespace SAPSec.Core.Constants;

/// <summary>
/// TypeOfResourcedProvision field values and helper methods.
/// Used to determine SEN unit status.
/// </summary>
public static class SenUnitValues
{
    public static readonly string NoSenUnitText = "Does not have a SEN unit";
    public static readonly string HasSenUnitText = "Has a SEN unit";
    public static readonly string NoDataAvailableText = "No data available";

    /// <summary>Values indicating no provision</summary>
    private static readonly string[] NoSenUnitValues =
    {
        "Not applicable",
        "resourced provision"
    };

    private static readonly string[] HasSenUnitValues =
{
        "resourced provision and SEN unit",
        "SEN unit"
    };

    /// <summary>
    /// Checks if the TypeOfResourcedProvision field indicates a SEN unit.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool HasSenUnit(string? provision)
    {
        if (string.IsNullOrWhiteSpace(provision))
            return false;

        foreach (var value in HasSenUnitValues)
        {
            if (provision.Contains(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool NoSenUnit(string? provision)
    {
        if (string.IsNullOrWhiteSpace(provision))
            return false;

        foreach (var value in NoSenUnitValues)
        {
            if (provision.Contains(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}