using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;

namespace SAPSec.Core.Features.SimilarSchools;

public record SimilarSchool
{
    public required string URN { get; set; }
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required BNGCoordinates? Coordinates { get; set; }
    public required int? TotalCapacity { get; set; }
    public required int? TotalPupils { get; set; }
    // TODO: convert into reference data (no ID in source data)
    public required string NurseryProvisionName { get; set; }
    public required ReferenceData LocalAuthority { get; set; }
    public required ReferenceData Region { get; set; }
    public required ReferenceData UrbanRural { get; set; }
    public required ReferenceData PhaseOfEducation { get; set; }
    public required ReferenceData OfficialSixthForm { get; set; }
    public required ReferenceData AdmissionsPolicy { get; set; }
    public required ReferenceData Gender { get; set; }
    public required ReferenceData ResourcedProvision { get; set; }
    public required ReferenceData TypeOfEstablishment { get; set; }
    public required ReferenceData EstablishmentTypeGroup { get; set; }
    public required ReferenceData TrustSchoolFlag { get; set; }
    public required DataWithAvailability<decimal> OverallAbsenceRate { get; set; }
    public required DataWithAvailability<decimal> PersistentAbsenceRate { get; set; }

    public static SimilarSchool FromData(Establishment currentEstab, EstablishmentAbsence? absence)
    {
        return new SimilarSchool
        {
            URN = currentEstab.URN,
            Name = currentEstab.EstablishmentName,
            Address = new Address
            {
                Street = currentEstab.Street,
                Locality = currentEstab.Locality,
                Address3 = currentEstab.Address3,
                Town = currentEstab.Town,
                Postcode = currentEstab.Postcode
            },
            TotalCapacity = currentEstab.TotalCapacity,
            TotalPupils = currentEstab.TotalPupils,
            NurseryProvisionName = currentEstab.NurseryProvisionName,
            Coordinates = BNGCoordinates.TryParse(currentEstab.Easting, currentEstab.Northing, out var coords) ? coords : null,
            LocalAuthority = new(currentEstab.LAId, currentEstab.LAName),
            UrbanRural = new(currentEstab.UrbanRuralId, currentEstab.UrbanRuralName),
            Region = new(currentEstab.RegionId, currentEstab.RegionName),
            AdmissionsPolicy = new(currentEstab.AdmissionsPolicyId, currentEstab.AdmissionsPolicyName),
            PhaseOfEducation = new(currentEstab.PhaseOfEducationId, currentEstab.PhaseOfEducationName),
            Gender = new(currentEstab.GenderId, currentEstab.GenderName),
            TypeOfEstablishment = new(currentEstab.TypeOfEstablishmentId, currentEstab.TypeOfEstablishmentName),
            EstablishmentTypeGroup = new(currentEstab.EstablishmentTypeGroupId, currentEstab.EstablishmentTypeGroupName),
            TrustSchoolFlag = new(currentEstab.TrustSchoolFlagId, currentEstab.TrustSchoolFlagName),
            OfficialSixthForm = new(currentEstab.OfficialSixthFormId, currentEstab.OfficialSixthFormName),
            ResourcedProvision = new(currentEstab.ResourcedProvisionId, currentEstab.ResourcedProvisionName),
            OverallAbsenceRate = DataWithAvailability.FromDecimalString(absence?.Abs_Tot_Est_Current_Pct),
            PersistentAbsenceRate = DataWithAvailability.FromDecimalString(absence?.Abs_Persistent_Est_Current_Pct)
        };
    }
}
