using SAPSec.Core.Model;
using SAPSec.Core.Model.Search;

namespace SAPSec.Core.Interfaces.Repositories
{
    public interface ISearchRepository
    {
        Task<IReadOnlyList<EstablishmentSearchResult>> SearchAsync(string query, int maxResults = 10);
        Task<IReadOnlyList<EstablishmentSearchResult>> SuggestAsync(string queryPart);
        Establishment? SearchByNumber(string schoolNumber);
    }
}
