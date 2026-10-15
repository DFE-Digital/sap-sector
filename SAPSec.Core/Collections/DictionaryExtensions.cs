namespace SAPSec.Core.Collections;

public static class DictionaryExtensions
{
    public static CaseInsensitiveDictionary<TValue> AsCaseInsensitive<TValue>(this IDictionary<string, TValue>? dict)
        => new CaseInsensitiveDictionary<TValue>(dict ?? new Dictionary<string, TValue>());
}
