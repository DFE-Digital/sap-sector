using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Data.Dto.RiseResources;

[ExcludeFromCodeCoverage]
public class RiseResourceCategoryEntry
{
    public string Category { get; set; } = string.Empty;
    public string CategoryDescription { get; set; } = string.Empty;
}
