using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Data.Dto.RiseResources;

[ExcludeFromCodeCoverage]
public class RiseResourcesDocument
{
    public IReadOnlyList<RiseResourceCategoryEntry> ResourceCategories { get; set; } = [];

    public IReadOnlyList<RiseResourceEntry> ResourceEntries { get; set; } = [];
}