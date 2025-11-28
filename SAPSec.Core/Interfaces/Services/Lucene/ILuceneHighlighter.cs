using Lucene.Net.Search;

namespace SAPSec.Core.Interfaces.Services.Lucene;

public interface ILuceneHighlighter
{
    string HighlightText(Query query, string searcher, string fieldName);
}