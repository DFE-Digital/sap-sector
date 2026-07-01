namespace SAPSec.Core.Constants;

/// <summary>
/// Trust school flag field values and helper methods.
/// Used to determine goverance structure of a school.
/// </summary>
public static class TrustSchoolFlagValues
{

    public const string NotApplicable = "0";
    public const string SupportedByTrust = "1";
    public const string NotSupportedByTrust = "2";
    public const string SupportedByMultiAcademyTrust = "3";
    public const string SupportedBySingleAcademyTrust = "5";


    public static bool IsSupportedBySingleAcademyTrust(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is SupportedBySingleAcademyTrust)
            return true;

        return false;

    }

    public static bool IsSupportedByMultiAcademyTrust(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is SupportedByMultiAcademyTrust)
            return true;

        return false;

    }

    public static bool IsSupportedByTrust(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is SupportedByTrust)
            return true;

        return false;

    }

    public static bool IsNotSupportedByTrust(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is NotSupportedByTrust)
            return true;

        return false;

    }

    public static bool IsNotApplicable(string trustSchoolFlagId)
    {
        var trimmedTrustSchoolFlagId = trustSchoolFlagId?.Trim();

        if (trimmedTrustSchoolFlagId is NotApplicable)
            return true;

        return false;

    }
}