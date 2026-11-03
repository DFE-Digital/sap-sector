namespace SAPSec.Web.ViewModels;

public static class AcademicYearLabelConfig
{
    public static readonly string[] YearByYear = ["2022 to 2023", "2023 to 2024", "2024 to 2025"];
    public static readonly string[] YearByYearLatest = ["2023 to 2024", "2024 to 2025", "2025 to 2026"];
    public static readonly string[] AttendanceYearByYear = ["2021 to 2022", "2022 to 2023", "2023 to 2024"];
    public static readonly string[] DestinationsYearByYear = ["2021 to 2022", "2022 to 2023", "2023 to 2024"];

    public static string CurrentKs2 => YearByYear[^1];
    public static string CurrentKs4 => YearByYearLatest[^1];
    public static string CurrentAttendance => AttendanceYearByYear[^1];
    public static string CurrentDestinations => DestinationsYearByYear[^1];

    public static string FormatAcademicYear(int year) => $"{year} to {year + 1}";
    public static string[] FormatYearByYear(int year) => [
        FormatAcademicYear(year - 2),
        FormatAcademicYear(year - 1),
        FormatAcademicYear(year)
    ];
}
