namespace SAPSec.Core.Interfaces.Services.Lucene;

public interface ILuceneIndexReader
{
    Task<IList<(int urn, string resultText)>> SearchAsync(string query);
}