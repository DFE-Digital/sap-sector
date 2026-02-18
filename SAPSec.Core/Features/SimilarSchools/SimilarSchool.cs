using SAPSec.Core.Features.Geography;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools;

public record SimilarSchool
{
    public required string URN { get; set; }
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required LocalAuthority LocalAuthority { get; set; }
    public required BNGCoordinates? Coordinates { get; set; }
    public required string UrbanRuralId { get; set; }
    public required string UrbanRuralName { get; set; }
    public required DataWithAvailability<decimal> Attainment8Score { get; set; }
    public required DataWithAvailability<decimal> BiologyGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> ChemistryGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> CombinedSciencGcseGrade55AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishLanguageGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishLiteratureGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> EnglishMathsGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> MathsGcseGrade5AndAbovePercentage { get; set; }
    public required DataWithAvailability<decimal> PhysicsGcseGrade5AndAbovePercentage { get; set; }
}
