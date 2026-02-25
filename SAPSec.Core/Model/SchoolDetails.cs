using SAPSec.Core.Constants;

namespace SAPSec.Core.Model;

/// <summary>
/// School details with data availability information.
/// Pure data container - all logic is in the service layer.
/// </summary>
public  class SchoolDetails
{
    // Identifiers
    public required string Urn { get; init; }
    public required string Name { get; init; }
    public required DataWithAvailability<string> DfENumber { get; init; }
    public required DataWithAvailability<string> Ukprn { get; init; }

    // Location
    public required DataWithAvailability<string> Address { get; init; }
    public required DataWithAvailability<string> LocalAuthorityName { get; init; }
    public required DataWithAvailability<string> LocalAuthorityCode { get; init; }
    public required DataWithAvailability<string> Region { get; init; }
    public required DataWithAvailability<string> UrbanRuralDescription { get; init; }

    // School characteristics
    public required DataWithAvailability<int> AgeRangeLow { get; init; }
    public required DataWithAvailability<int> AgeRangeHigh { get; init; }
    public required DataWithAvailability<string> GenderOfEntry { get; init; }
    public required DataWithAvailability<string> PhaseOfEducation { get; init; }
    public required DataWithAvailability<string> SchoolType { get; init; }
    public required DataWithAvailability<string> AdmissionsPolicy { get; init; }
    public required DataWithAvailability<string> ReligiousCharacter { get; init; }

    // Governance
    public required DataWithAvailability<GovernanceType> GovernanceStructure { get; init; }
    public required DataWithAvailability<string> AcademyTrustName { get; init; }
    public required DataWithAvailability<string> AcademyTrustId { get; init; }

    // Provisions
    public required DataWithAvailability<bool> HasNurseryProvision { get; init; }
    public required DataWithAvailability<bool> HasSixthForm { get; init; }
    public required DataWithAvailability<bool> HasSenUnit { get; init; }
    public required DataWithAvailability<bool> HasResourcedProvision { get; init; }

    // Contact
    public required DataWithAvailability<string> HeadteacherName { get; init; }
    public required DataWithAvailability<string> Website { get; init; }
    public required DataWithAvailability<string> Telephone { get; init; }
    public required DataWithAvailability<string> Email { get; init; }
}