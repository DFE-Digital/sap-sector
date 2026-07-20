namespace SAPSec.PrimaryJsonFileGenerator;

internal class JsonGeneratorSettings
{
    public string[] UrnsWithDataUnderTenPercent { get; set; } = [];
    public string[] UrnsWithMissingCurrentYearData { get; set; } = [];
    public string[] UrnsWithMissingPreviousYearData { get; set; } = [];
    public string[] UrnsWithMissingPrevious2YearData { get; set; } = [];
}
