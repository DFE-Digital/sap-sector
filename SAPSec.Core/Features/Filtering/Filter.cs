namespace SAPSec.Core.Features.Filtering;

public record Filter(
    string Key,
    string Name,
    FilterType Type,
    IReadOnlyCollection<FilterOption> Options);
