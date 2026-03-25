using SAPSec.Core.Features.Geography;
using SAPSec.Core.Model;
using SAPSec.Data.Model.Generated;

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
    public required DataWithAvailability<decimal> Attainment8Score { get; set; }
    public required DataWithAvailability<decimal> BiologyGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> ChemistryGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> CombinedScienceGcseGrade55AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishLanguageGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishLiteratureGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishMathsGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> MathsGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> PhysicsGcseGrade5AndAbovePercentage { get; set; }

    public static SimilarSchool FromData(Establishment currentEstab, IEnumerable<EstablishmentPerformance> currentSchoolPerformances)
    {
        var currentSchoolPerformance = currentSchoolPerformances.FirstOrDefault();
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
            Attainment8Score = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Attainment8_Tot_Est_Current_Num),
            BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Bio59_Sum_Est_Current_Num),
            ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Chem59_Sum_Est_Current_Num),
            CombinedScienceGcseGrade55AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.CombSci59_Sum_Est_Current_Num),
            EnglishLanguageGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.EngLang59_Sum_Est_Current_Num),
            EnglishLiteratureGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.EngLit59_Sum_Est_Current_Num),
            EnglishMathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.EngMaths59_Tot_Est_Current_Num),
            MathsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Maths59_Sum_Est_Current_Num),
            PhysicsGcseGrade5AndAbovePercentage = DataWithAvailability.FromDecimalString(currentSchoolPerformance?.Physics59_Sum_Est_Current_Num),
        };
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0m;
        }

        return decimal.TryParse(
            value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : 0m;
    }

    private static int ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return int.TryParse(
            value,
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : 0;
    }
}
