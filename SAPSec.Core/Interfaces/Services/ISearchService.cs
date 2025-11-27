using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Interfaces.Services;

public interface ISearchService
{
    Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query);
    Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart);
    School? SearchByNumber(string schoolNumber);
    School GetSchoolByUrn(int urn);
}
