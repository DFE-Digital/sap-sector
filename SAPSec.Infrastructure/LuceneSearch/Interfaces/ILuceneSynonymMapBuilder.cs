using Lucene.Net.Analysis.Synonym;

namespace SAPSec.Infrastructure.LuceneSearch.Interfaces;

public interface ILuceneSynonymMapBuilder
{
    SynonymMap BuildSynonymMap();
}