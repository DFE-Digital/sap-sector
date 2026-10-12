namespace SAPSec.Core.Features.Measures.Attendance;

public record GetComparisonAttendanceMeasuresResponse(
    SchoolInfo.School CurrentSchool,
    SchoolInfo.School ComparatorSchool,
    Measure Absence);
