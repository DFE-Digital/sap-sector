using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// School details with data availability information.
/// Pure data container - all logic is in the service layer.
/// </summary>
public class SchoolDetails
    : LinkedSchool
{
    private SchoolDetails(LinkedSchool school)
        : base(school)
    {
    }

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

    public string OfstedProviderId => IsAllThroughSchool
        ? GetAllThroughOfstedProviderId()
        : IsPrimarySchool ? "21" : "23";

    private string GetAllThroughOfstedProviderId() =>
        TypeOfEstablishmentCode.IsAvailable && TypeOfEstablishmentCode.Value == "49"
            ? "100003"
            : "28";

    public static SchoolDetails Create(
        Establishment establishment,
        SchoolClosureEligibility closureEligibility,
        SchoolPredecessorRelationship predecessorRelationship,
        EstablishmentEmail? establishmentEmail,
        DataWithAvailability<GovernanceType> governanceStructure,
        DataWithAvailability<bool> hasNurseryProvision,
        DataWithAvailability<bool> hasSixthForm,
        DataWithAvailability<bool> hasSenUnit,
        DataWithAvailability<bool> hasResourcedProvision)
    {
        return new SchoolDetails(
            Create(establishment, closureEligibility, predecessorRelationship))
        {
            // Identifiers
            DfENumber = DataMapper.MapDfENumber(establishment),
            Ukprn = DataMapper.MapRequiredString(establishment.UKPRN),

            // Location
            Region = DataMapper.MapString(establishment.DistrictAdministrativeName),
            UrbanRuralDescription = DataMapper.MapString(establishment.UrbanRuralName),

            // School characteristics
            AgeRangeLow = DataWithAvailability.FromNullable(establishment.AgeRangeLow),
            AgeRangeHigh = DataWithAvailability.FromNullable(establishment.AgeRangeHigh),
            GenderOfEntry = DataMapper.MapString(establishment.GenderName),
            PhaseOfEducation = DataMapper.MapString(establishment.PhaseOfEducationName),
            SchoolType = DataMapper.MapString(establishment.TypeOfEstablishmentName),
            TypeOfEstablishmentCode = DataMapper.MapRequiredString(establishment.TypeOfEstablishmentId),
            AdmissionsPolicy = DataMapper.MapString(establishment.AdmissionsPolicyName),
            ReligiousCharacter = DataMapper.MapString(establishment.ReligiousCharacterName),

            // Governance - business rule
            GovernanceStructure = governanceStructure,
            AcademyTrustName = DataMapper.MapTrustName(establishment),
            AcademyTrustId = DataMapper.MapTrustId(establishment.TrustId),

            // Provisions - business rules
            HasNurseryProvision = hasNurseryProvision,
            HasSixthForm = hasSixthForm,
            HasSenUnit = hasSenUnit,
            HasResourcedProvision = hasResourcedProvision,

            // Contact
            HeadteacherName = DataMapper.MapHeadteacher(establishment),
            Website = DataMapper.MapWebsite(establishment.Website),
            Telephone = DataMapper.MapString(establishment.TelephoneNum),
            Email = DataMapper.MapString(establishmentEmail?.MainEmail)
        };
    }
}
