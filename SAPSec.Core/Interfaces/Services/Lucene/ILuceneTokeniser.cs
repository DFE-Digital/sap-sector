namespace SAPSec.Core.Interfaces.Services.Lucene;

public interface ILuceneTokeniser
{
    IEnumerable<string> Tokenise(string finalText);
}