namespace SAPSec.Core.Constants;

/// <summary>
/// Trust school flag field values and helper methods.
/// Used to determine goverance structure of a school.
/// </summary>
public static class TrustSchoolFlagValues
{
    public static readonly string SingleAcademyTrust = "Single-academy Trust";
    public static readonly string MultiAcademyTrust = "Multi-academy Trust";
    public static readonly string LaMaintainedSchool = "Maintained school - local authority controlled";
    public static readonly string NoKnownGroup = "No known group";

    public const string NotApplicable = "0";
    public const string SupportedByTrust = "1";
    public const string NotSupportedByTrust = "2";
    public const string SupportedByMultiAcademyTrust = "3";
    public const string SupportedBySingleAcademyTrust = "5";

    /// <summary>
    /// Checks if the school is supported by a single academy trust.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IsSupportedBySingleAcademyTrust(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is SupportedBySingleAcademyTrust)
            return true;

        return false;
    }

    /// <summary>
    /// Checks if the school is supported by a multi-academy trust.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IsSupportedByMultiAcademyTrust(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is SupportedByMultiAcademyTrust)
            return true;

        return false;
    }

    /// <summary>
    /// Checks if the school is supported by a trust.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IsSupportedByTrust(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is SupportedByTrust)
            return true;

        return false;
    }

    /// <summary>
    /// Checks if the school is not supported by a trust.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IsNotSupportedByTrust(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is NotSupportedByTrust)
            return true;

        return false;
    }

    /// <summary>
    /// Checks if the school is not applicable.
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IsNotApplicable(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is NotApplicable)
            return true;

        return false;
    }
}