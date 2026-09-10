namespace SAPSec.Core.Features.SchoolDetails.Comparison;

public record GetComparisonSchoolDetailsResponse(
    SchoolWithCoordinates CurrentSchool,
    SchoolWithCoordinates ComparatorSchool,
    double? DistanceMiles,
    SchoolDetails ComparatorSchoolDetails);
