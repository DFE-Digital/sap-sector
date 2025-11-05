namespace SAPSec.Core.Interfaces.Services;

public interface IQueryTokeniser
{
    IEnumerable<string> Tokenise(string finalText);
}