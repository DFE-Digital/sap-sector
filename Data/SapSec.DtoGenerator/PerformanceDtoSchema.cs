using SAPData.Models;

namespace SAPSec.DtoGenerator;

public static class PerformanceDtoSchema
{
    public static void IncludeMappedProperties(
        IDictionary<string, object?> properties,
        IEnumerable<DataMapRow> dataMap,
        string className,
        string subFolder)
    {
        if (!className.EndsWith("Performance", StringComparison.Ordinal)
            || subFolder is not ("KS4\\Performance" or "KS2\\Performance"))
        {
            return;
        }

        var range = className[..^"Performance".Length];
        var type = subFolder.Replace('\\', '_');
        var mappedProperties = dataMap
            .Where(row => row.Range == range && row.Type == type)
            .Where(row => !string.IsNullOrWhiteSpace(row.PropertyName))
            .Where(row => !string.Equals(row.IgnoreMapping?.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
            .Select(row => row.PropertyName)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (mappedProperties.Count == 0)
        {
            throw new InvalidOperationException($"No data-map properties found for {type}/{range}.");
        }

        foreach (var property in mappedProperties)
        {
            // Metric values remain strings so suppression codes are preserved, as in the JSON snapshots.
            properties.TryAdd(property, null);
        }
    }
}
