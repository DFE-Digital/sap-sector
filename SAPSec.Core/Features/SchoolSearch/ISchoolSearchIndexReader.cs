namespace SAPSec.Core.Features.SchoolSearch
{
    public interface ISchoolSearchIndexReader
    {
        Task<IList<(int urn, string resultText)>> SearchAsync(string query, int maxResults = 10);
    }
}
