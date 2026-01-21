namespace SAPSec.Core.Constants;

/// <summary>
/// Classification of establishment types by governance category.
/// Uses GIAS TypeOfEstablishment IDs.
/// </summary>
public static class EstablishmentTypeClassification
{
    #region Type ID Sets

    /// <summary>Academy and free school type IDs</summary>
    public static readonly IReadOnlySet<string> AcademyTypeIds = new HashSet<string>
    {
        "28",  // Academy sponsor led
        "33",  // Academy special sponsor led
        "34",  // Academy converter
        "35",  // Free schools
        "36",  // Free schools special
        "38",  // Free schools alternative provision
        "40",  // University technical college
        "41",  // Studio schools
        "42",  // Academy alternative provision converter
        "43",  // Academy alternative provision sponsor led
        "44",  // Academy special converter
        "45",  // Academy 16-19 converter
        "46",  // Academy 16-19 sponsor led
        "47",  // Free schools 16-19
        "57"   // Academy secure 16 to 19
    };

    /// <summary>Local authority maintained school type IDs</summary>
    public static readonly IReadOnlySet<string> LocalAuthorityMaintainedTypeIds = new HashSet<string>
    {
        "1",   // Community school
        "2",   // Voluntary aided school
        "3",   // Voluntary controlled school
        "5",   // Foundation school
        "7",   // Community special school
        "12",  // Foundation special school
        "14",  // Pupil referral unit
        "15",  // LA nursery school
        "24",  // Secure units
        "25",  // Offshore schools
        "26",  // Service children's education
        "30",  // Welsh establishment
        "31",  // Sixth form centres
        "37",  // British schools overseas
        "39",  // Miscellaneous
    };

    /// <summary>Independent school type IDs</summary>
    public static readonly IReadOnlySet<string> IndependentTypeIds = new HashSet<string>
    {
        "10",  // Other independent school
        "11",  // Other independent special school
    };

    /// <summary>Further/Higher education type IDs</summary>
    public static readonly IReadOnlySet<string> FurtherEducationTypeIds = new HashSet<string>
    {
        "18",  // Further education
        "29",  // Higher education institutions
        "32",  // Special post 16 institution
    };

    /// <summary>Non-maintained special school type IDs</summary>
    public static readonly IReadOnlySet<string> NonMaintainedSpecialSchoolTypeIds = new HashSet<string>
    {
        "8",   // Non-maintained special school
    };

    #endregion

    #region Type Name Patterns (for fallback detection)

    /// <summary>Name patterns that indicate academy/free school types</summary>
    private static readonly string[] AcademyNamePatterns =
    {
        "Academy",
        "Free school",
        "Studio school",
        "University technical college"
    };

    #endregion

    #region Classification Methods

    /// <summary>Checks if the establishment type ID is an academy or free school</summary>
    public static bool IsAcademy(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && AcademyTypeIds.Contains(typeId);

    /// <summary>Checks if the establishment type ID is LA maintained</summary>
    public static bool IsLocalAuthorityMaintained(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && LocalAuthorityMaintainedTypeIds.Contains(typeId);

    /// <summary>Checks if the establishment type ID is independent</summary>
    public static bool IsIndependent(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && IndependentTypeIds.Contains(typeId);

    /// <summary>Checks if the establishment type ID is further/higher education</summary>
    public static bool IsFurtherEducation(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && FurtherEducationTypeIds.Contains(typeId);

    /// <summary>Checks if the establishment type ID is a non-maintained special school</summary>
    public static bool IsNonMaintainedSpecialSchool(string? typeId)
        => !string.IsNullOrWhiteSpace(typeId) && NonMaintainedSpecialSchoolTypeIds.Contains(typeId);

    /// <summary>
    /// Checks if the establishment type name indicates an academy (fallback when ID is unknown).
    /// Uses case-insensitive comparison.
    /// </summary>
    public static bool IsAcademyByName(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return false;

        foreach (var pattern in AcademyNamePatterns)
        {
            if (typeName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    #endregion
}