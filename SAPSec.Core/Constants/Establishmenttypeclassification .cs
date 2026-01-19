namespace SAPSec.Core.Constants;

/// <summary>
/// Encapsulates establishment type classification rules.
/// Single source of truth for categorising school types.
/// </summary>
public static class EstablishmentTypeClassification
{
    /// <summary>
    /// Academy and Free School establishment type IDs.
    /// </summary>
    public static readonly IReadOnlySet<string> AcademyTypeIds = new HashSet<string>
    {
        "28",  // Academy sponsor led
        "33",  // Academy special sponsor led
        "34",  // Academy converter
        "35",  // Free schools
        "36",  // Free schools special
        "38",  // Free schools alternative provision
        "39",  // Free schools 16 to 19
        "40",  // University technical college
        "41",  // Studio schools
        "42",  // Academy alternative provision converter
        "43",  // Academy alternative provision sponsor led
        "44",  // Academy special converter
        "45",  // Academy 16-19 converter
        "46",  // Academy 16 to 19 sponsor led
        "57"   // Academy secure 16 to 19
    };

    /// <summary>
    /// Local Authority maintained establishment type IDs.
    /// </summary>
    public static readonly IReadOnlySet<string> LocalAuthorityTypeIds = new HashSet<string>
    {
        "1",   // Community school
        "2",   // Voluntary aided school
        "3",   // Voluntary controlled school
        "5",   // Foundation school
        "7",   // Community special school
        "12",  // Foundation special school
        "14",  // Pupil referral unit
        "15"   // Local authority nursery school
    };

    /// <summary>
    /// Independent school establishment type IDs.
    /// </summary>
    public static readonly IReadOnlySet<string> IndependentTypeIds = new HashSet<string>
    {
        "10",  // Other independent special school
        "11"   // Other independent school
    };

    /// <summary>
    /// Further/Higher education establishment type IDs.
    /// </summary>
    public static readonly IReadOnlySet<string> FurtherEducationTypeIds = new HashSet<string>
    {
        "18",  // Further education
        "29",  // Higher education institutions
        "32"   // Special post 16 institution
    };

    /// <summary>Non-maintained special school type ID</summary>
    public const string NonMaintainedSpecialSchoolTypeId = "8";

    /// <summary>
    /// Checks if the establishment type is an Academy or Free School.
    /// </summary>
    public static bool IsAcademy(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && AcademyTypeIds.Contains(typeId);

    /// <summary>
    /// Checks if the establishment type is LA maintained.
    /// </summary>
    public static bool IsLocalAuthorityMaintained(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && LocalAuthorityTypeIds.Contains(typeId);

    /// <summary>
    /// Checks if the establishment type is Independent.
    /// </summary>
    public static bool IsIndependent(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && IndependentTypeIds.Contains(typeId);

    /// <summary>
    /// Checks if the establishment type is Further/Higher Education.
    /// </summary>
    public static bool IsFurtherEducation(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && FurtherEducationTypeIds.Contains(typeId);

    /// <summary>
    /// Checks if the establishment type is a Non-maintained special school.
    /// </summary>
    public static bool IsNonMaintainedSpecialSchool(string? typeId)
        => typeId == NonMaintainedSpecialSchoolTypeId;
}