using SAPSec.Data.Dto;

namespace SAPSec.Core.School.Search;

public interface ISchoolSearchService
{
    Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query);
    Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart);
    Task<Establishment?> SearchByNumberAsync(string schoolNumber);
}
