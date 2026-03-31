using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Model;
using SAPSec.Web.Helpers;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public class SchoolAttendancePageViewModel
{
    public required SchoolDetails SchoolDetails { get; init; }
    public required GetAttendanceMeasuresResponse AttendanceMeasures { get; init; }

    public string SchoolName => SchoolDetails.Name;
    public string LocalAuthorityName => SchoolDetails.LocalAuthorityName.Display();
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";
    public string[] AcademicYears => Ks4YearLabelConfig.YearByYear;

    public decimal? SelectedSchoolOverallAbsenceThreeYearAverage => AttendanceMeasures.OverallAbsenceThreeYearAverage.SchoolValue;
    public decimal? LocalAuthorityOverallAbsenceThreeYearAverage => AttendanceMeasures.OverallAbsenceThreeYearAverage.LocalAuthorityValue;
    public decimal? EnglandOverallAbsenceThreeYearAverage => AttendanceMeasures.OverallAbsenceThreeYearAverage.EnglandValue;

    public decimal? SelectedSchoolPersistentAbsenceThreeYearAverage => AttendanceMeasures.PersistentAbsenceThreeYearAverage.SchoolValue;
    public decimal? LocalAuthorityPersistentAbsenceThreeYearAverage => AttendanceMeasures.PersistentAbsenceThreeYearAverage.LocalAuthorityValue;
    public decimal? EnglandPersistentAbsenceThreeYearAverage => AttendanceMeasures.PersistentAbsenceThreeYearAverage.EnglandValue;

    public decimal OverallAbsenceBarAxisMax => CalculateAxisMax(
        [
            SelectedSchoolOverallAbsenceThreeYearAverage,
            LocalAuthorityOverallAbsenceThreeYearAverage,
            EnglandOverallAbsenceThreeYearAverage
        ],
        AttendanceAxisCalculator.OverallAbsence);

    public decimal OverallAbsenceLineAxisMax => CalculateAxisMax(
        [
            AttendanceMeasures.OverallAbsenceYearByYear.School.Previous2,
            AttendanceMeasures.OverallAbsenceYearByYear.School.Previous,
            AttendanceMeasures.OverallAbsenceYearByYear.School.Current,
            AttendanceMeasures.OverallAbsenceYearByYear.LocalAuthority.Previous2,
            AttendanceMeasures.OverallAbsenceYearByYear.LocalAuthority.Previous,
            AttendanceMeasures.OverallAbsenceYearByYear.LocalAuthority.Current,
            AttendanceMeasures.OverallAbsenceYearByYear.England.Previous2,
            AttendanceMeasures.OverallAbsenceYearByYear.England.Previous,
            AttendanceMeasures.OverallAbsenceYearByYear.England.Current
        ],
        AttendanceAxisCalculator.OverallAbsence);

    public decimal PersistentAbsenceBarAxisMax => CalculateAxisMax(
        [
            SelectedSchoolPersistentAbsenceThreeYearAverage,
            LocalAuthorityPersistentAbsenceThreeYearAverage,
            EnglandPersistentAbsenceThreeYearAverage
        ],
        AttendanceAxisCalculator.PersistentAbsence);

    public decimal PersistentAbsenceLineAxisMax => CalculateAxisMax(
        [
            AttendanceMeasures.PersistentAbsenceYearByYear.School.Previous2,
            AttendanceMeasures.PersistentAbsenceYearByYear.School.Previous,
            AttendanceMeasures.PersistentAbsenceYearByYear.School.Current,
            AttendanceMeasures.PersistentAbsenceYearByYear.LocalAuthority.Previous2,
            AttendanceMeasures.PersistentAbsenceYearByYear.LocalAuthority.Previous,
            AttendanceMeasures.PersistentAbsenceYearByYear.LocalAuthority.Current,
            AttendanceMeasures.PersistentAbsenceYearByYear.England.Previous2,
            AttendanceMeasures.PersistentAbsenceYearByYear.England.Previous,
            AttendanceMeasures.PersistentAbsenceYearByYear.England.Current
        ],
        AttendanceAxisCalculator.PersistentAbsence);

    public static string DisplayPercentNullable(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.00", CultureInfo.InvariantCulture) + "%"
            : "No available data";

    public static decimal CalculateAxisMax(IEnumerable<decimal?> values, AttendanceAxisSettings settings) =>
        AttendanceAxisCalculator.CalculateMax(values, settings);
}
