using System.Text.Json.Serialization;

namespace SAPSec.Core.Pagination;

public class ResultsPage<T>
{
    [JsonConstructor]
    private ResultsPage()
    {
        AllItems = new List<T>();
        Items = new List<T>();
    }

    public ResultsPage(IEnumerable<T> allItems, int currentPage, int itemsPerPage)
    {
        AllItems = allItems;
        TotalCount = allItems.Count();

        ItemsPerPage = itemsPerPage;

        var lastPage = Math.Max(1, (int)Math.Ceiling(TotalCount / (decimal)ItemsPerPage));
        CurrentPage = Math.Clamp(currentPage, 1, lastPage);

        Items = allItems
            .Skip((CurrentPage - 1) * ItemsPerPage)
            .Take(ItemsPerPage)
            .ToList();
        Count = Items.Count();
    }

    [JsonInclude]
    public IEnumerable<T> Items { get; private set; }

    public IEnumerable<T> AllItems { get; private set; }

    [JsonInclude]
    public int CurrentPage { get; private set; }

    [JsonInclude]
    public int ItemsPerPage { get; private set; }

    [JsonInclude]
    public int TotalCount { get; private set; }

    [JsonInclude]
    public int Count { get; private set; }

    [JsonIgnore]
    public int TotalPages => (int)Math.Ceiling(TotalCount / (decimal)ItemsPerPage);

    public ResultsPage<U> Map<U>(Func<T, U> mapItem) => new ResultsPage<U>()
    {
        Items = Items.Select(mapItem).ToList(),
        CurrentPage = CurrentPage,
        ItemsPerPage = ItemsPerPage,
        TotalCount = TotalCount,
        Count = Count
    };
}
