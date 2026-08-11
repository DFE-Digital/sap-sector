namespace SAPSec.Core.Collections;

public class CaseInsensitiveDictionary<TValue> : Dictionary<string, TValue>
{
    public CaseInsensitiveDictionary(IDictionary<string, TValue> source) 
        : base(source, StringComparer.OrdinalIgnoreCase)
    {
    }
}
