using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Data.Dto.RiseResources;

[ExcludeFromCodeCoverage]
public class RiseResourceEntry
{
    public string ResourceTitle { get; set; } = string.Empty;
    public string ResourceDescription { get; set; } = string.Empty;
    public string ResourceUrl { get; set; } = string.Empty;
    public IReadOnlyList<string> SchoolPhases { get; set; } = [];
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public IReadOnlyList<string> MappingMeasures { get; set; } = [];
}
