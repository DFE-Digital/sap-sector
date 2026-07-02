namespace SAPSec.Core.Constants;

/// <summary>
/// Resourced provision field values and helper methods.
/// Used to determine SEN unit and resourced provision status.
/// </summary>
public static class ResourcedProvisionValues
{
    public static readonly string NoResourcedProvisionText = "Does not have a resourced provision";
    public static readonly string HasResourcedProvisionText = "Has a resourced provision";
    public static readonly string NoDataAvailableText = "No data available";

    #region Provision Patterns

    /// <summary>Values indicating no provision</summary>
    private static readonly string[] HasResourcedProvisionValues =
    {
        "Resourced provision",
        "Resourced provision and SEN unit"
    };

    private static readonly string[] NoResourcedProvisionValues =
{
        "Not applicable",
        "SEN unit"
    };

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if the TypeOfResourcedProvision field indicates a resourced provision.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool HasResourcedProvision(string? provision)
    {
        if (string.IsNullOrWhiteSpace(provision))
            return false;

        foreach (var value in HasResourcedProvisionValues)
        {
            if (provision.Contains(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the TypeOfResourcedProvision field indicates no resourced provision.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool NoResourcedProvision(string? provision)
    {
        if (string.IsNullOrWhiteSpace(provision))
            return false;

        foreach (var value in NoResourcedProvisionValues)
        {
            if (provision.Equals(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    #endregion
}