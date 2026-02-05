using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SchoolSearch;

public interface ISchoolSearchService
{
    Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query);
    Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart);
    Establishment? SearchByNumber(string schoolNumber);
}
