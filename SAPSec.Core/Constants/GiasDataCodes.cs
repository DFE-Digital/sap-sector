namespace SAPSec.Core.Constants;

/// <summary>
/// GIAS special data codes indicating data availability/quality.
/// These codes appear in EES data fields to indicate special states.
/// </summary>
public static class EesDataCodes
{
    /// <summary>Data has been redacted (withheld for privacy)</summary>
    public const string Redacted = "c";

    /// <summary>Data is not applicable for this establishment type</summary>
    public const string NotApplicable = "z";

    /// <summary>Data is not available/missing</summary>
    public const string NotAvailable = "x";

    /// <summary>Data quality is low</summary>
    public const string LowQuality = "low";

    /// <summary>
    /// Checks if a value matches the redacted code (case-insensitive).
    /// </summary>
    public static bool IsRedacted(string? value)
        => string.Equals(value, Redacted, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if a value matches the not applicable code (case-insensitive).
    /// </summary>
    public static bool IsNotApplicable(string? value)
        => string.Equals(value, NotApplicable, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if a value matches the not available code (case-insensitive).
    /// </summary>
    public static bool IsNotAvailable(string? value)
        => string.Equals(value, NotAvailable, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if a value matches the low quality code (case-insensitive).
    /// </summary>
    public static bool IsLowQuality(string? value)
        => string.Equals(value, LowQuality, StringComparison.OrdinalIgnoreCase);
}