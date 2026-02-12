namespace SAPSec.Core.Features.Pagination;

public interface IPagedCollection<T> : IReadOnlyCollection<T>
{
    int CurrentPage { get; }
    int ItemsPerPage { get; }
    int TotalCount { get; }

    int TotalPages => (int)Math.Ceiling(TotalCount / (decimal)ItemsPerPage);
    IPagedCollection<U> Map<U>(Func<T, U> mapItem);
}
