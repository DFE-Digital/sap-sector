using SAPSec.Core.Attendance;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public class SchoolAttendancePageViewModel
{
    public record TopPerformerRow(int Rank, string Urn, string Name, decimal? Value, string DisplayValue, bool IsCurrentSchool);

    public required SchoolInfoViewModel School { get; init; }
    public required GetAttendanceMeasuresResponse AttendanceMeasures { get; init; }

    public string SchoolName => School.Name;
    public string SimilarSchoolsLabel => "Similar schools average";
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";
    public string[] AcademicYears => Ks4YearLabelConfig.YearByYear;

    public decimal? SelectedSchoolOverallAbsenceThreeYearAverage => AttendanceMeasures.OverallAbsenceThreeYearAverage.SchoolValue;
    public decimal? SimilarSchoolsOverallAbsenceThreeYearAverage => AttendanceMeasures.OverallAbsenceThreeYearAverage.SimilarSchoolsValue;
    public decimal? LocalAuthorityOverallAbsenceThreeYearAverage => AttendanceMeasures.OverallAbsenceThreeYearAverage.LocalAuthorityValue;
    public decimal? EnglandOverallAbsenceThreeYearAverage => AttendanceMeasures.OverallAbsenceThreeYearAverage.EnglandValue;

    public decimal? SelectedSchoolPersistentAbsenceThreeYearAverage => AttendanceMeasures.PersistentAbsenceThreeYearAverage.SchoolValue;
    public decimal? SimilarSchoolsPersistentAbsenceThreeYearAverage => AttendanceMeasures.PersistentAbsenceThreeYearAverage.SimilarSchoolsValue;
    public decimal? LocalAuthorityPersistentAbsenceThreeYearAverage => AttendanceMeasures.PersistentAbsenceThreeYearAverage.LocalAuthorityValue;
    public decimal? EnglandPersistentAbsenceThreeYearAverage => AttendanceMeasures.PersistentAbsenceThreeYearAverage.EnglandValue;

    public IReadOnlyList<TopPerformerRow> OverallAbsenceTopPerformers =>
        AttendanceMeasures.OverallAbsenceTopPerformers
            .Select(x => new TopPerformerRow(x.Rank, x.Urn, x.Name, x.Value, DisplayPercentNullable(x.Value), x.IsCurrentSchool))
            .ToList()
            .AsReadOnly();

    public IReadOnlyList<TopPerformerRow> PersistentAbsenceTopPerformers =>
        AttendanceMeasures.PersistentAbsenceTopPerformers
            .Select(x => new TopPerformerRow(x.Rank, x.Urn, x.Name, x.Value, DisplayPercentNullable(x.Value), x.IsCurrentSchool))
            .ToList()
            .AsReadOnly();

    public static string DisplayPercentNullable(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.00", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
