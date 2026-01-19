namespace SAPSec.Core.Constants;

/// <summary>
/// Constants for GIAS special data codes.
/// These codes indicate data quality/availability status.
/// </summary>
public static class GiasDataCodes
{
    /// <summary>Data has been redacted for privacy</summary>
    public const string Redacted = "c";

    /// <summary>Data is not applicable for this school type</summary>
    public const string NotApplicable = "z";

    /// <summary>Data is not available/missing</summary>
    public const string NotAvailable = "x";

    /// <summary>Data quality is low</summary>
    public const string LowQuality = "low";

    /// <summary>
    /// Checks if a value is a special GIAS code.
    /// </summary>
    public static bool IsSpecialCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var lower = value.ToLower();
        return lower is Redacted or NotApplicable or NotAvailable or LowQuality;
    }
}