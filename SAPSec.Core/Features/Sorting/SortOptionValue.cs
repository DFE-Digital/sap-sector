namespace SAPSec.Core.Features.Sorting;

public record SortOptionValue<T>(
    string Key,
    string Name,
    T Value);

public record SortedItem<TItem, TValue>(
    TItem Item,
    SortOptionValue<TValue> Value);