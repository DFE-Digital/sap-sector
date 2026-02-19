using SAPSec.Core.Features.Geography;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools;

public record SimilarSchool
{
    public required string URN { get; set; }
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required BNGCoordinates? Coordinates { get; set; }
    public required string TotalCapacity { get; set; }
    public required string TotalPupils { get; set; }
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
    public required DataWithAvailability<decimal> Attainment8Score { get; set; }
    public required DataWithAvailability<decimal> BiologyGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> ChemistryGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> CombinedScienceGcseGrade55AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishLanguageGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishLiteratureGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishMathsGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> MathsGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> PhysicsGcseGrade5AndAbovePercentage { get; set; }
}