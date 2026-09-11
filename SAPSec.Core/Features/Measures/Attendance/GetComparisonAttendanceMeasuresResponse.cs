namespace SAPSec.Core.Features.Measures.Attendance;

public record GetComparisonAttendanceMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo ComparatorSchool,
    Measure Absence);
