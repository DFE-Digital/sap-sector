using SAPSec.Core.Features.Availability;
using SAPSec.Core.Geography;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;

namespace SAPSec.Core.Features.SimilarSchools;

public record SimilarSchool
{
    public required SchoolInfo.School Info { get; init; }
    public required BNGCoordinates? Coordinates { get; init; }
    public required int? TotalCapacity { get; init; }
    public required int? TotalPupils { get; init; }
    // TODO: convert into reference data (no ID in source data)
    public required string NurseryProvisionName { get; init; }
    public required ReferenceData Region { get; init; }
    public required ReferenceData UrbanRural { get; init; }
    public required ReferenceData PhaseOfEducation { get; init; }
    public required ReferenceData OfficialSixthForm { get; init; }
    public required ReferenceData AdmissionsPolicy { get; init; }
    public required ReferenceData Gender { get; init; }
    public required ReferenceData ResourcedProvision { get; init; }
    public required ReferenceData TypeOfEstablishment { get; init; }
    public required ReferenceData EstablishmentTypeGroup { get; init; }
    public required ReferenceData TrustSchoolFlag { get; init; }
    public required DataWithAvailability<decimal> OverallAbsenceRate { get; init; }
    public required DataWithAvailability<decimal> PersistentAbsenceRate { get; init; }

    public static SimilarSchool Create(Establishment establishment, EstablishmentAbsence? absence)
    {
        return new SimilarSchool
        {
            Info = SchoolInfo.School.Create(establishment),
            TotalCapacity = establishment.TotalCapacity,
            TotalPupils = establishment.TotalPupils,
            NurseryProvisionName = establishment.NurseryProvisionName,
            Coordinates = BNGCoordinates.TryParse(establishment.Easting, establishment.Northing, out var coords) ? coords : null,
            UrbanRural = new(establishment.UrbanRuralId, establishment.UrbanRuralName),
            Region = new(establishment.RegionId, establishment.RegionName),
            AdmissionsPolicy = new(establishment.AdmissionsPolicyId, establishment.AdmissionsPolicyName),
            PhaseOfEducation = new(establishment.PhaseOfEducationId, establishment.PhaseOfEducationName),
            Gender = new(establishment.GenderId, establishment.GenderName),
            TypeOfEstablishment = new(establishment.TypeOfEstablishmentId, establishment.TypeOfEstablishmentName),
            EstablishmentTypeGroup = new(establishment.EstablishmentTypeGroupId, establishment.EstablishmentTypeGroupName),
            TrustSchoolFlag = new(establishment.TrustSchoolFlagId, establishment.TrustSchoolFlagName),
            OfficialSixthForm = new(establishment.OfficialSixthFormId, establishment.OfficialSixthFormName),
            ResourcedProvision = new(establishment.ResourcedProvisionId, establishment.ResourcedProvisionName),
            OverallAbsenceRate = DataWithAvailability.FromDecimalString(absence?.Abs_Tot_Est_Current_Pct),
            PersistentAbsenceRate = DataWithAvailability.FromDecimalString(absence?.Abs_Persistent_Est_Current_Pct)
        };
    }
}
