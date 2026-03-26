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

    public static string DisplayPercentNullable(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.00", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
