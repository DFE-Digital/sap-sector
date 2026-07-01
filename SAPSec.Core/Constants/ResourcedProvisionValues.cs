namespace SAPSec.Core.Constants;

/// <summary>
/// Resourced provision field values and helper methods.
/// Used to determine SEN unit and resourced provision status.
/// </summary>
public static class ResourcedProvisionValues
{
    #region Provision Patterns

    /// <summary>Values indicating no provision</summary>
    private static readonly string[] HasResourcedProvisionValues =
    {
        "resourced provision",
        "resourced provision and SEN unit"
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
            if (provision.Contains(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    #endregion
}