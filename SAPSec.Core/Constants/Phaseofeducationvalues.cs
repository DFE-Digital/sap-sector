namespace SAPSec.Core.Constants;

/// <summary>
/// Constants for PhaseOfEducation values and indicators.
/// </summary>
public static class PhaseOfEducationValues
{
    /// <summary>Indicators that a school has nursery provision</summary>
    public static readonly string[] NurseryIndicators = { "nursery" };

    /// <summary>Indicators that a school definitely doesn't have nursery</summary>
    public static readonly string[] NoNurseryIndicators =
    {
        "secondary",
        "16 plus",
        "post-16",
        "16-19"
    };

    /// <summary>Indicators where nursery status is indeterminate</summary>
    public static readonly string[] IndeterminateIndicators = { "all-through" };

    /// <summary>Primary phase indicator</summary>
    public const string Primary = "primary";

    /// <summary>
    /// Checks if the phase indicates nursery provision.
    /// </summary>
    public static bool IndicatesNursery(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
            return false;

        var lower = phase.ToLower();
        return NurseryIndicators.Any(lower.Contains);
    }

    /// <summary>
    /// Checks if the phase indicates no nursery provision.
    /// </summary>
    public static bool IndicatesNoNursery(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
            return false;

        var lower = phase.ToLower();
        return NoNurseryIndicators.Any(lower.Contains) || lower.Contains(Primary);
    }

    /// <summary>
    /// Checks if the nursery status is indeterminate for this phase.
    /// </summary>
    public static bool IsIndeterminate(string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
            return true;

        var lower = phase.ToLower();
        return IndeterminateIndicators.Any(lower.Contains);
    }
}