namespace SAPSec.Core.FeatureFlags;

public static class Flags
{
    public const string EnablePrimarySchools = "EnablePrimarySchools";
    public const string EnableAllThroughSchools = "EnableAllThroughSchools";
    public const string EnableRiseResources = "EnableRiseResources";
    public const string EnableKs4CoreSubjectsPupilCharacteristics = "EnableKs4CoreSubjectsPupilCharacteristics";

    /// <summary>
    /// Shows example chart pages built from made-up data, for reviewing chart designs with product/design.
    /// Must stay off in production.
    /// </summary>
    public const string EnableChartExamples = "EnableChartExamples";
}
