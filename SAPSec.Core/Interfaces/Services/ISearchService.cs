using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Interfaces.Services;

public interface ISearchService
{
    Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int take = 10);
    Task<IReadOnlyList<string>> SuggestAsync(string prefix, int take = 10);
}
