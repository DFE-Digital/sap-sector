namespace SAPSec.Core.Features.Filtering;

public record FilterOption(
    string Key,
    string Name,
    bool Selected,
    int Count = 0);
