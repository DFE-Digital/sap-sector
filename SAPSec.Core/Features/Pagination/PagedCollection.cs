using System.Collections;

namespace SAPSec.Core.Features.Pagination;

public class PagedCollection<T> : IPagedCollection<T>
{
    private IList<T> _allItems;
    private IList<T> _items;

    public PagedCollection(IEnumerable<T> allItems, int currentPage, int itemsPerPage)
    {
        _allItems = allItems.ToList();
        TotalCount = _allItems.Count;
        ItemsPerPage = itemsPerPage;

        var lastPage = Math.Max(1, (int)Math.Ceiling(TotalCount / (decimal)ItemsPerPage));
        CurrentPage = Math.Clamp(currentPage, 1, lastPage);

        _items = allItems
            .Skip((CurrentPage - 1) * ItemsPerPage)
            .Take(ItemsPerPage)
            .ToList();

        Count = _items.Count;
    }

    public int CurrentPage { get; }
    public int ItemsPerPage { get; }
    public int TotalCount { get; }
    public int Count { get; }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IPagedCollection<U> Map<U>(Func<T, U> mapItem) => new PagedCollection<U>(
        _allItems.Select(mapItem).ToList(),
        CurrentPage,
        ItemsPerPage);

}
