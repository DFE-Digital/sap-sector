namespace SAPSec.Core.Constants;

/// <summary>
/// Phase of education values and helper methods.
/// Used to determine provisions like nursery.
/// </summary>
public static class NurseryProvisionValues
{
    public static readonly string HasNurseryClassesText = "Has nursery classes";
    public static readonly string NoNurseryClassesText = "Does not have nursery classes";

    #region Nursery Classes Patterns

    public const string SchoolHasNurseryClasses = "Has Nursery Classes";


    /// <summary>Patterns that indicate nursery provision</summary>
    private static readonly string[] SchoolHasNoNurseryClasses =
    {
        "Not applicable",
        "No Nursery Classes"
    };

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if the school has nursery classes.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool HasNurseryClasses(string? nurseryProvision)
    {
        if (string.IsNullOrWhiteSpace(nurseryProvision))
            return false;

        if (nurseryProvision.Contains(SchoolHasNurseryClasses, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    /// <summary>
    /// Checks if the school does not have nursery classes.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool NoNurseryClasses(string? nurseryProvision)
    {
        if (string.IsNullOrWhiteSpace(nurseryProvision))
            return false;

        foreach (var pattern in SchoolHasNoNurseryClasses)
        {
            if (nurseryProvision.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    #endregion
}
