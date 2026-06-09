using SAPSec.Data.Dto;

namespace SAPSec.Core.SchoolSearch;

public interface ISchoolSearchService
{
    Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query);
    Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart);
    Task<Establishment?> SearchByNumberAsync(string schoolNumber);
}
