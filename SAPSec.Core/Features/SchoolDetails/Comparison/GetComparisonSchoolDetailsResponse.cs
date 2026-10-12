using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Core.Features.SchoolDetails.Comparison;

public record GetComparisonSchoolDetailsResponse(
    SchoolWithCoordinates CurrentSchool,
    SchoolWithCoordinates ComparatorSchool,
    double? DistanceMiles,
    SchoolInfoWithLinks ComparatorSchoolInfoWithLinks,
    SchoolDetails ComparatorSchoolDetails);
