using Microsoft.AspNetCore.Http;
using SAPSec.Core.Caching;
using System.Collections;
using System.Reflection;

namespace SAPSec.Core.UseCases;

public class CachedUseCase<TRequest, TResponse>(IUseCase<TRequest, TResponse> useCase, ICache cache)
    : IUseCase<TRequest, TResponse>
{
    public Task<TResponse> Execute(TRequest request)
    {
        var properties = typeof(TRequest)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .OrderBy(p => p.Name)
            .ToList();

        var key = GetStringValue(request);

        return cache.TryGet($"{useCase.GetType().FullName}:{key}", () => useCase.Execute(request));
    }

    private string GetStringValue(object? value)
    {
        return value switch
        {
            null => "",
            string s => s,
            IDictionary dict => SerializeDictionary(dict),
            IEnumerable list => SerializeEnumerable(list),
            bool or int or short or long or float or double or decimal or char or byte => value.ToString() ?? "",
            object o => SerializeObject(o)
        };
    }

    private string SerializeDictionary(IDictionary dict)
        => KeyValueString(dict.Keys.Cast<object>().Select(k => (k.ToString() ?? "", dict[k])));

    private string SerializeEnumerable(IEnumerable list)
        => KeyValueString(list.Cast<object?>().Select((item, i) => (i.ToString(), item)));

    private string SerializeObject(object o)
        => KeyValueString(o.GetType().GetProperties().Select(p => (p.Name, p.GetValue(o))));

    private string KeyValueString(IEnumerable<(string, object?)> items)
    {
        var qs = new QueryString();

        foreach (var (k, v) in items)
        {
            qs = qs.Add(k, GetStringValue(v));
        }

        return qs.Value;
    }
}
