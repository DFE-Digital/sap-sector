namespace SAPSec.Core.Constants;

/// <summary>
/// Constants for ResourcedProvision field values.
/// </summary>
public static class ResourcedProvisionValues
{
    /// <summary>Indicates school has a SEN unit</summary>
    public const string SenUnit = "sen unit";

    /// <summary>Indicates school has resourced provision</summary>
    public const string ResourcedProvision = "resourced provision";

    /// <summary>No provision available</summary>
    public const string NotApplicable = "not applicable";

    /// <summary>No provision</summary>
    public const string None = "none";

    /// <summary>
    /// Checks if the provision value indicates no provision.
    /// </summary>
    public static bool IsNoProvision(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var lower = value.ToLower();
        return lower is NotApplicable or None;
    }

    /// <summary>
    /// Checks if the provision value indicates a SEN unit.
    /// </summary>
    public static bool HasSenUnit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.ToLower().Contains(SenUnit);
    }

    /// <summary>
    /// Checks if the provision value indicates resourced provision.
    /// </summary>
    public static bool HasResourcedProvision(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.ToLower().Contains(ResourcedProvision);
    }
}