namespace SAPSec.Infrastructure.LuceneSearch.Interfaces;

public interface ILuceneIndexReader
{
    Task<IList<(int urn, string resultText)>> SearchAsync(string query);
}