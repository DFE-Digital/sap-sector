using System.Collections.Concurrent;
using System.Text;

namespace SAPSec.Web.Errors;

public interface IErrorStore
{
    string Add(Exception ex);
    string? Get(string id);
}

public class InMemoryErrorStore : IErrorStore
{
    private readonly ConcurrentDictionary<string, string> _store = new();

    public string Add(Exception ex)
    {
        var id = Guid.NewGuid().ToString("N");
        _store[id] = BuildExceptionDetails(ex);
        return id;
    }

    public string? Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _store.TryGetValue(id, out var details) ? details : null;
    }

    private static string BuildExceptionDetails(Exception ex)
    {
        var sb = new StringBuilder();
        var current = ex;
        var level = 0;
        while (current != null)
        {
            sb.AppendLine($"Level {level}: {current.GetType().FullName}: {current.Message}");
            if (!string.IsNullOrEmpty(current.StackTrace))
            {
                sb.AppendLine(current.StackTrace);
            }
            sb.AppendLine();
            current = current.InnerException;
            level++;
        }
        return sb.ToString();
    }
}