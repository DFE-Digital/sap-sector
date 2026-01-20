namespace SAPSec.Core.Constants;

/// <summary>
/// Phase of education values and helper methods.
/// Used to determine provisions like nursery.
/// </summary>
public static class PhaseOfEducationValues
{
    #region Phase Patterns

    /// <summary>Patterns that indicate nursery provision</summary>
    private static readonly string[] NurseryPatterns =
    {
        "Nursery"
    };

    /// <summary>Patterns that indicate no nursery provision</summary>
    private static readonly string[] NoNurseryPatterns =
    {
        "Primary",
        "Secondary",
        "16 plus",
        "Post-16",
        "16-19"
    };

    /// <summary>Patterns where nursery provision is indeterminate</summary>
    private static readonly string[] IndeterminatePatterns =
    {
        "All-through",
        "All through",
        "Middle"
    };

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if the phase indicates nursery provision.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IndicatesNursery(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
            return false;

        foreach (var pattern in NurseryPatterns)
        {
            if (phase.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the phase indicates no nursery provision.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IndicatesNoNursery(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
            return false;

        foreach (var pattern in NoNurseryPatterns)
        {
            if (phase.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the phase is indeterminate for nursery provision.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IsIndeterminate(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
            return false;

        foreach (var pattern in IndeterminatePatterns)
        {
            if (phase.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    #endregion
}