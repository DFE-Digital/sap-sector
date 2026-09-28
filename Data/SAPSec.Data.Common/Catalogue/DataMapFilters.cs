using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>A data map row's filters (Filter..Filter9) as column and values; "a+b" means a or b.</summary>
public static class DataMapFilters
{
    public static IEnumerable<(string Column, string[] Values)> Of(DataMapRow r) =>
        new[]
        {
            (r.Filter, r.FilterValue), (r.Filter2, r.Filter2Value), (r.Filter3, r.Filter3Value),
            (r.Filter4, r.Filter4Value), (r.Filter5, r.Filter5Value), (r.Filter6, r.Filter6Value),
            (r.Filter7, r.Filter7Value), (r.Filter8, r.Filter8Value), (r.Filter9, r.Filter9Value)
        }
        .Where(f => !string.IsNullOrWhiteSpace(f.Item1))
        .Select(f => (f.Item1.Trim(), (f.Item2 ?? "").Split('+')));
}
