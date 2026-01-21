namespace SAPSec.Core.Model;

/// <summary>
/// School governance structure types.
/// Determined by business logic based on establishment type and trust membership.
/// </summary>
public enum GovernanceType
{
    /// <summary>Part of a Multi-Academy Trust</summary>
    MultiAcademyTrust,

    /// <summary>Single Academy Trust (standalone academy)</summary>
    SingleAcademyTrust,

    /// <summary>Maintained by Local Authority</summary>
    LocalAuthorityMaintained,

    /// <summary>Non-maintained special school</summary>
    NonMaintainedSpecialSchool,

    /// <summary>Independent school</summary>
    Independent,

    /// <summary>Further or Higher Education institution</summary>
    FurtherHigherEducation,

    /// <summary>Other governance type</summary>
    Other
}