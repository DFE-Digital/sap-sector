using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Util;
using SAPSec.Core.Interfaces.Services.Lucene;

namespace SAPSec.Core.Services.Lucene;

public class LuceneSynonymMapBuilder : ILuceneSynonymMapBuilder
{
    private static readonly List<string[]> SynonymGroups =
    [
        ["st", "ss", "saint"],
        ["cofe", "church of england"],
        ["rm", "roman catholic"],
        ["ave", "ave.", "aven", "avenue"],
        ["rd", "road"],
        ["ln", "lane"],
        ["dr", "drive"],
        ["mt", "mount"],
        ["ct", "court"],
        ["pl", "place"],
        ["blvd", "boulevard"],
        ["sq", "square"]
    ];

    public SynonymMap BuildSynonymMap()
    {
        var builder = new SynonymMap.Builder(true); //dedup = true

        foreach (var synonymGroup in SynonymGroups)
        {
            for (var i = 0; i < synonymGroup.Length; i++)
            {
                // Split multi-word synonyms into term arrays and join them properly for Lucene's synonym map
                var inputTerms = synonymGroup[i]
                    .ToLowerInvariant()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for (var j = 0; j < synonymGroup.Length; j++)
                {
                    if (i == j) continue; // Skip mapping a term to itself

                    var outputTerms = synonymGroup[j]
                        .ToLowerInvariant()
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    var inputChars = SynonymMap.Builder.Join(inputTerms, new CharsRef());
                    var outputChars = SynonymMap.Builder.Join(outputTerms, new CharsRef());

                    builder.Add(inputChars, outputChars, true);
                }
            }
        }

        return builder.Build();
    }
}