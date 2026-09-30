namespace SAPSec.Web.ViewModels;

public static class AcademicYearLabelConfig
{
    public static string FormatAcademicYear(int year) => $"{year} to {year + 1}";
    public static string[] FormatYearByYear(int year) => [
        FormatAcademicYear(year - 2),
        FormatAcademicYear(year - 1),
        FormatAcademicYear(year)
    ];
}
