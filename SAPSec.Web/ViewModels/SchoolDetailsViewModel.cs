using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class SchoolDetailsViewModel
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

    public static SchoolDetailsViewModel FromSchoolDetails(SchoolDetails schoolDetails) =>
        new()
        {
            Urn = schoolDetails.Urn,
            Name = schoolDetails.Name,
            DfENumber = schoolDetails.DfENumber,
            Ukprn = schoolDetails.Ukprn,
            Address = schoolDetails.Address,
            LocalAuthorityName = schoolDetails.LocalAuthorityName,
            LocalAuthorityCode = schoolDetails.LocalAuthorityCode,
            Region = schoolDetails.Region,
            UrbanRuralDescription = schoolDetails.UrbanRuralDescription,
            AgeRangeLow = schoolDetails.AgeRangeLow,
            AgeRangeHigh = schoolDetails.AgeRangeHigh,
            GenderOfEntry = schoolDetails.GenderOfEntry,
            PhaseOfEducation = schoolDetails.PhaseOfEducation,
            SchoolType = schoolDetails.SchoolType,
            AdmissionsPolicy = schoolDetails.AdmissionsPolicy,
            ReligiousCharacter = schoolDetails.ReligiousCharacter,
            GovernanceStructure = schoolDetails.GovernanceStructure,
            AcademyTrustName = schoolDetails.AcademyTrustName,
            AcademyTrustId = schoolDetails.AcademyTrustId,
            HasNurseryProvision = schoolDetails.HasNurseryProvision,
            HasSixthForm = schoolDetails.HasSixthForm,
            HasSenUnit = schoolDetails.HasSenUnit,
            HasResourcedProvision = schoolDetails.HasResourcedProvision,
            HeadteacherName = schoolDetails.HeadteacherName,
            Website = schoolDetails.Website,
            Telephone = schoolDetails.Telephone,
            Email = schoolDetails.Email,
        };
}