namespace SAPSec.Core.Constants;

/// <summary>
/// Phase of education values and helper methods.
/// Used to determine provisions like nursery.
/// </summary>
public static class PhaseOfEducationValues
{
    public const string NotApplicableId = "0";
    public const string NurseryId = "1";
    public const string PrimaryId = "2";
    public const string MiddleDeemedPrimaryId = "3";
    public const string SecondaryId = "4";
    public const string MiddleDeemedSecondaryId = "5";
    public const string SixteenPlusId = "6";
    public const string AllThroughId = "7";

    public const string Primary = "Primary";
    public const string Secondary = "Secondary";
    public const string AllThrough = "All-through";

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

    public static bool IsPrimaryOrAllThrough(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
            return false;

        var trimmedPhase = phase.Trim();

        return string.Equals(trimmedPhase, Primary, StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmedPhase, AllThrough, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsSecondary(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
            return false;

        return string.Equals(phase.Trim(), Secondary, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsSearchableIndexPhaseId(string? phaseId)
    {
        var trimmedPhaseId = phaseId?.Trim();

        return trimmedPhaseId is PrimaryId or SecondaryId or AllThroughId;
    }

    public static bool IsSearchable(string? phaseId, string? phaseName, bool primarySchoolsEnabled)
    {
        var trimmedPhaseId = phaseId?.Trim();

        return trimmedPhaseId switch
        {
            SecondaryId => true,
            PrimaryId or AllThroughId => primarySchoolsEnabled,
            _ => IsSecondary(phaseName)
                || (primarySchoolsEnabled && IsPrimaryOrAllThrough(phaseName))
        };
    }

    #endregion
}
