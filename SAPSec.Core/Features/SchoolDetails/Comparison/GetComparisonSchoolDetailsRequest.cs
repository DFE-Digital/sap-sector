namespace SAPSec.Core.Features.SchoolDetails.Comparison;

public record GetComparisonSchoolDetailsRequest(
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn);
