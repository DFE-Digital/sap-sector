using Lucene.Net.Search;

namespace SAPSec.Infrastructure.LuceneSearch.Interfaces;

public interface ILuceneHighlighter
{
    string HighlightText(Query query, string searcher, string fieldName);
}