using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.SchoolSearch;

public interface ISchoolSearchService
{
    Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query, bool primarySchoolsEnabled = false);
    Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart, bool primarySchoolsEnabled = false);
    Task<Establishment?> SearchByNumberAsync(string schoolNumber, bool primarySchoolsEnabled = false);
}
