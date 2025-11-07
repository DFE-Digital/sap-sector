namespace SAPSec.Core.Interfaces.Services;

public interface IAbbreviationExpander
{
    /// <summary>
    /// Expand query text by normalizing abbreviations, common variants, and producing synonyms.
    /// Returns a set of unique expanded terms suitable for search.
    /// </summary>
    string ExpandTerms(string queryText);
}
