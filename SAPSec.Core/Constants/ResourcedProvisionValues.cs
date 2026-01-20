namespace SAPSec.Core.Constants;

/// <summary>
/// Resourced provision field values and helper methods.
/// Used to determine SEN unit and resourced provision status.
/// </summary>
public static class ResourcedProvisionValues
{
    #region Provision Patterns

    /// <summary>Pattern indicating SEN unit</summary>
    private const string SenUnitPattern = "SEN unit";

    /// <summary>Pattern indicating resourced provision</summary>
    private const string ResourcedProvisionPattern = "resourced provision";

    /// <summary>Values indicating no provision</summary>
    private static readonly string[] NoProvisionValues =
    {
        "Not applicable",
        "None"
    };

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if the provision field indicates a SEN unit.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool HasSenUnit(string? provision)
    {
        if (string.IsNullOrWhiteSpace(provision))
            return false;

        return provision.Contains(SenUnitPattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the provision field indicates resourced provision.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool HasResourcedProvision(string? provision)
    {
        if (string.IsNullOrWhiteSpace(provision))
            return false;

        return provision.Contains(ResourcedProvisionPattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the provision field indicates no provision.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IsNoProvision(string? provision)
    {
        if (string.IsNullOrWhiteSpace(provision))
            return true;

        foreach (var value in NoProvisionValues)
        {
            if (provision.Contains(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    #endregion
}