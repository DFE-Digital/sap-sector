using SAPSec.Core.Model;
using SAPSec.Core.Model.Search;

namespace SAPSec.Core.Interfaces.Services;

public interface ISearchService
{
    Task<IReadOnlyList<EstablishmentSearchResult>> SearchAsync(string query);
    Task<IReadOnlyList<EstablishmentSearchResult>> SuggestAsync(string queryPart);
    Establishment? SearchByNumber(string schoolNumber);
}
