namespace SAPSec.Infrastructure.LuceneSearch.Interfaces;

public interface ILuceneTokeniser
{
    IEnumerable<string> Tokenise(string finalText);
}