namespace SAPSec.Core.FeatureFlags;

public static class Flags
{
    public const string EnablePrimarySchools = "EnablePrimarySchools";
    public const string EnableAllThroughSchools = "EnableAllThroughSchools";
    public const string EnableRiseResources = "EnableRiseResources";
    public const string EnableKs4CoreSubjectsPupilCharacteristics = "EnableKs4CoreSubjectsPupilCharacteristics";
    public const string EnableFindASchool = "EnableFindASchool";
    public const string EnableKs2ProgressNumberOfPupils = "EnableKs2ProgressNumberOfPupils";
    public const string EnableKs2ProgressMeasures = "EnableKs2ProgressMeasures";

    /// <summary>
    /// TESTING ONLY - remove when the 2025 to 2026 KS2 progress data is loaded.
    /// Only 2022 to 2023 has published progress scores, so this shows those scores as the latest year
    /// (bar chart, top performers and latest-year column) without changing the year labels.
    /// Must stay off on test, preapproval and production.
    /// </summary>
    public const string Ks2ProgressUseEarliestYearAsLatest = "Ks2ProgressUseEarliestYearAsLatest";
}
