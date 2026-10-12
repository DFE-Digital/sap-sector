using SAPSec.Core.Features.Availability;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// School details with data availability information.
/// Pure data container - all logic is in the service layer.
/// </summary>
public class SchoolDetails
{
    public required SchoolInfo.SchoolInfo Info { get; init; }

    // Identifiers
    public required DataWithAvailability<string> DfENumber { get; init; }
    public required DataWithAvailability<string> Ukprn { get; init; }

    // Location
    public required DataWithAvailability<string> Region { get; init; }
    public required DataWithAvailability<string> UrbanRuralDescription { get; init; }

    // School characteristics
    public required DataWithAvailability<int> AgeRangeLow { get; init; }
    public required DataWithAvailability<int> AgeRangeHigh { get; init; }
    public required DataWithAvailability<string> GenderOfEntry { get; init; }
    public required DataWithAvailability<string> PhaseOfEducation { get; init; }
    public required DataWithAvailability<string> SchoolType { get; init; }
    public required DataWithAvailability<string> TypeOfEstablishmentCode { get; init; }
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

    public string OfstedProviderId => Info.IsAllThroughSchool
        ? GetAllThroughOfstedProviderId()
        : Info.IsPrimarySchool ? "21" : "23";

    private string GetAllThroughOfstedProviderId() =>
        TypeOfEstablishmentCode.IsAvailable && TypeOfEstablishmentCode.Value == "49"
            ? "100003"
            : "28";

}
