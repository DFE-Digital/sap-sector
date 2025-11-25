using Lucene.Net.Analysis.Synonym;

namespace SAPSec.Core.Interfaces.Services.Lucene;

public interface ILuceneSynonymMapBuilder
{
    SynonymMap BuildSynonymMap();
}