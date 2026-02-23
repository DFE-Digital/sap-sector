namespace SAPSec.Core.Features.Filtering;

public record FilterOption(
    string Key,
    string Name,
    int Count,
    bool Selected);
