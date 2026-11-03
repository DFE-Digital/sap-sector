using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Web.Areas.Shared.ViewModels.SimilarSchools;

/// <summary>
/// Filter/tag-building logic shared between the primary and secondary
/// "View similar schools" pages - the underlying <see cref="SimilarSchoolsAvailableFilter"/>
/// shape is identical for both phases.
/// </summary>
public static class SimilarSchoolsViewModelHelpers
{
    // Range filters measured per 100 pupils rather than as a percentage
    private static readonly HashSet<string> Per100PupilsRangeKeys = new(["sr", "per"], StringComparer.InvariantCultureIgnoreCase);

    public static bool IsPercentageRange(string key) => !Per100PupilsRangeKeys.Contains(key);

    public static List<SimilarSchoolsFilterGroupViewModel> BuildFilterGroups(
        IReadOnlyCollection<SimilarSchoolsAvailableFilter> filterOptions)
    {
        var categoryKeys = new List<(string Heading, List<string> Keys)>
        {
            ("Location", ["dist", "reg", "ur"]),
            ("School characteristics", ["st", "poe", "sciu", "gs", "np", "sf", "ap", "sp", "goe"]),
            ("Attendance and exclusion", ["oar", "par", "sr", "pps", "per"])
        };

        var grouped = new List<SimilarSchoolsFilterGroupViewModel>();

        foreach (var (heading, keys) in categoryKeys)
        {
            var filters = keys
                .Select(key => filterOptions.FirstOrDefault(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
                .Where(f => f is not null)
                .Cast<SimilarSchoolsAvailableFilter>()
                .ToList();

            if (filters.Any())
            {
                grouped.Add(new SimilarSchoolsFilterGroupViewModel(heading, filters));
            }
        }

        var knownKeys = categoryKeys.SelectMany(kvp => kvp.Keys).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        var remaining = filterOptions.Where(f => !knownKeys.Contains(f.Key)).ToList();
        if (remaining.Any())
        {
            grouped.Add(new SimilarSchoolsFilterGroupViewModel("Other filters", remaining));
        }

        return grouped;
    }

    public static List<SimilarSchoolsSelectedFilterTagViewModel> BuildSelectedFilterTags(
        IReadOnlyCollection<SimilarSchoolsAvailableFilter> filterOptions,
        Dictionary<string, List<string>> currentFilters,
        string sortBy,
        string baseUrl)
    {
        var tags = new List<SimilarSchoolsSelectedFilterTagViewModel>();

        foreach (var filter in filterOptions)
        {
            if (filter is SimilarSchoolsSingleValueAvailableFilter single)
            {
                foreach (var option in single.Options.Where(o => o.Selected))
                {
                    var queryString = BuildQueryStringWithout(currentFilters, sortBy, [(filter.Key, option.Key)]);
                    tags.Add(new SimilarSchoolsSelectedFilterTagViewModel(option.Name, AppendQueryString(baseUrl, queryString)));
                }
            }

            if (filter is SimilarSchoolsMultiValueAvailableFilter multi)
            {
                foreach (var option in multi.Options.Where(o => o.Selected))
                {
                    var queryString = BuildQueryStringWithout(currentFilters, sortBy, [(filter.Key, option.Key)]);
                    tags.Add(new SimilarSchoolsSelectedFilterTagViewModel(option.Name, AppendQueryString(baseUrl, queryString)));
                }
            }

            if (filter is SimilarSchoolsNumericRangeAvailableFilter range
                && !range.ValidationErrors.Any()
                && (!string.IsNullOrWhiteSpace(range.From.Value) || !string.IsNullOrWhiteSpace(range.To.Value)))
            {
                IEnumerable<(string, string)> exclude = [
                    (range.From.Key, range.From.Value),
                    (range.To.Key, range.To.Value)
                ];
                var queryString = BuildQueryStringWithout(currentFilters, sortBy, exclude);
                var unit = IsPercentageRange(range.Key) ? "%" : "";
                var rangeText = (string.IsNullOrWhiteSpace(range.From.Value), string.IsNullOrWhiteSpace(range.To.Value)) switch
                {
                    (false, false) => $"from {range.From.Value}{unit} to {range.To.Value}{unit}",
                    (false, true) => $"over {range.From.Value}{unit}",
                    (true, false) => $"up to {range.To.Value}{unit}",
                    _ => ""
                };
                tags.Add(new SimilarSchoolsSelectedFilterTagViewModel($"{range.Name} {rangeText}".Trim(), AppendQueryString(baseUrl, queryString)));
            }
        }

        return tags;
    }

    public static string BuildQueryStringWithout(
        Dictionary<string, List<string>> currentFilters,
        string sortBy,
        IEnumerable<(string Key, string Value)> exclude)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            parts.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        }

        foreach (var (key, values) in currentFilters)
        {
            foreach (var value in values)
            {
                if (exclude.Any(e => key.Equals(e.Key, StringComparison.InvariantCultureIgnoreCase)
                    && value.Equals(e.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                parts.Add($"{key}={Uri.EscapeDataString(value)}");
            }
        }

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private static string AppendQueryString(string baseUrl, string queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return baseUrl;
        }

        return baseUrl.Contains('?')
            ? baseUrl + "&" + queryString.TrimStart('?')
            : baseUrl + queryString;
    }
}
