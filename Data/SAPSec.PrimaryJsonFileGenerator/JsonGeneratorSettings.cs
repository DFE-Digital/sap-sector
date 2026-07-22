namespace SAPSec.PrimaryJsonFileGenerator;

// Settings class mapping to entire appsettings.json file
internal class JsonGeneratorSettings
{
    public PropertyDataSpec[] Properties { get; set; } = [];
}

// Property specs constraining data generation - if multiple are found applying to the same URN or property,
// the first one in the list is used
internal class PropertyDataSpec
{
    /// Urns this spec applies to - if empty/missing applies to all URNs
    public string[] Urns { get; set; } = [];

    // Properties this spec applies to - if empty/missing applies to all properties
    public string[] PropertyNamePatterns { get; set; } = [];

    // Min value of randomly generated data for the property - if empty/missing defaults to 0
    public double? MinValue { get; set; }

    // Max value of randomly generated data for the property - if empty/missing defaults to 100
    public double? MaxValue { get; set; }

    // Whether the property is empty - if empty/missing defaults to false (overrides MaxValue and MinValue)
    public bool Empty { get; set; }
}
