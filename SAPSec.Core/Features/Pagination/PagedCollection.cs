using System.Collections;

namespace SAPSec.Core.Features.Pagination;

public class PagedCollection<T>(IList<T> allItems, int currentPage, int itemsPerPage) : IPagedCollection<T>
{
    private IList<T> _items = allItems
        .Skip((currentPage - 1) * itemsPerPage)
        .Take(itemsPerPage)
        .ToList();

    public int CurrentPage => currentPage;
    public int ItemsPerPage => itemsPerPage;
    public int TotalCount => allItems.Count;
    public int Count => _items.Count;
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    public IPagedCollection<U> Map<U>(Func<T, U> mapItem) => new PagedCollection<U>(
        allItems.Select(mapItem).ToList(),
        currentPage,
        itemsPerPage);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
