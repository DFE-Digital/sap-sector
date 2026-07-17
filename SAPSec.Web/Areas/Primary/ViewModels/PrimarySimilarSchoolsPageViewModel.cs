using Microsoft.AspNetCore.Http;
using SAPSec.Core;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using System.Globalization;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public class PrimarySimilarSchoolsPageViewModel
{
    public SchoolInfoViewModel CurrentSchool { get; init; } = null!;
    public string CurrentSchoolLocalAuthorityName { get; init; } = string.Empty;
    public PrimarySimilarSchoolsCharacteristicsViewModel CurrentSchoolCharacteristics { get; init; } = null!;
    public IReadOnlyCollection<PrimarySimilarSchoolsRowViewModel> SimilarSchools { get; init; } = [];
    public int Urn { get; init; }
    public Dictionary<string, List<string>> CurrentFilters { get; init; } = new(StringComparer.InvariantCultureIgnoreCase);
    public List<SimilarSchoolsFilterGroupViewModel> FilterGroups { get; init; } = [];
    public List<SimilarSchoolsSelectedFilterTagViewModel> SelectedFilterTags { get; init; } = [];
    public IReadOnlyCollection<ValidationError> ValidationErrors { get; init; } = [];

    public int TotalResults => SimilarSchools.Count;
    public bool HasActiveFilters => SelectedFilterTags.Any();

    public static PrimarySimilarSchoolsPageViewModel FromResponse(
        FindPrimarySimilarSchoolsResponse response,
        IQueryCollection query)
    {
        var currentFilters = ExtractCurrentFilters(query);
        var filterOptions = response.FilterOptions;

        return new PrimarySimilarSchoolsPageViewModel
        {
            CurrentSchool = new SchoolInfoViewModel(
                response.CurrentSchool.Urn,
                response.CurrentSchool.Name,
                string.Empty),
            CurrentSchoolLocalAuthorityName = response.CurrentSchool.LocalAuthorityName,
            CurrentSchoolCharacteristics = PrimarySimilarSchoolsCharacteristicsViewModel.FromResponse(response.CurrentSchool.Characteristics),
            SimilarSchools = response.SimilarSchools
                .Select(row => new PrimarySimilarSchoolsRowViewModel(
                    row.Urn,
                    row.Name,
                    row.LocalAuthorityName,
                    row.Rank,
                    row.Distance,
                    Routes.PrimarySchool(response.CurrentSchool.Urn).SimilarSchoolComparison(row.Urn),
                    PrimarySimilarSchoolsCharacteristicsViewModel.FromResponse(row.Characteristics)))
                .ToList()
                .AsReadOnly(),
            Urn = int.TryParse(response.CurrentSchool.Urn, out var urn) ? urn : 0,
            CurrentFilters = currentFilters,
            FilterGroups = BuildFilterGroups(filterOptions),
            SelectedFilterTags = BuildSelectedFilterTags(filterOptions, currentFilters, response.CurrentSchool.Urn),
            ValidationErrors = response.ValidationErrors
        };
    }

    private static Dictionary<string, List<string>> ExtractCurrentFilters(IQueryCollection query)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var (key, values) in query)
        {
            if (key == "sortBy" || key == "page")
            {
                continue;
            }

            result[key] = values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).ToList();
        }

        return result;
    }

    private static List<SimilarSchoolsFilterGroupViewModel> BuildFilterGroups(
        IReadOnlyCollection<SimilarSchoolsAvailableFilter> filterOptions)
    {
        var categoryKeys = new List<(string Heading, List<string> Keys)>
        {
            ("Location", ["dist", "reg", "ur"]),
            ("School characteristics", ["st", "poe", "sciu", "gs", "np", "sf", "ap", "sp", "goe"]),
            ("Attendance", ["oar", "par"])
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

        return grouped;
    }

    private static List<SimilarSchoolsSelectedFilterTagViewModel> BuildSelectedFilterTags(
        IReadOnlyCollection<SimilarSchoolsAvailableFilter> filterOptions,
        Dictionary<string, List<string>> currentFilters,
        string urn)
    {
        var tags = new List<SimilarSchoolsSelectedFilterTagViewModel>();
        var baseUrl = Routes.PrimarySchool(urn).ViewSimilarSchools;

        foreach (var filter in filterOptions)
        {
            if (filter is SimilarSchoolsSingleValueAvailableFilter single)
            {
                foreach (var option in single.Options.Where(o => o.Selected))
                {
                    var queryString = BuildQueryStringWithout(currentFilters, [(filter.Key, option.Key)]);
                    tags.Add(new SimilarSchoolsSelectedFilterTagViewModel(option.Name, baseUrl + queryString));
                }
            }

            if (filter is SimilarSchoolsMultiValueAvailableFilter multi)
            {
                foreach (var option in multi.Options.Where(o => o.Selected))
                {
                    var queryString = BuildQueryStringWithout(currentFilters, [(filter.Key, option.Key)]);
                    tags.Add(new SimilarSchoolsSelectedFilterTagViewModel(option.Name, baseUrl + queryString));
                }
            }

            if (filter is SimilarSchoolsNumericRangeAvailableFilter range
                && (!string.IsNullOrWhiteSpace(range.From.Value) || !string.IsNullOrWhiteSpace(range.To.Value)))
            {
                var queryString = BuildQueryStringWithout(currentFilters,
                [
                    (range.From.Key, range.From.Value),
                    (range.To.Key, range.To.Value)
                ]);

                var rangeText = (string.IsNullOrWhiteSpace(range.From.Value), string.IsNullOrWhiteSpace(range.To.Value)) switch
                {
                    (false, false) => $"from {range.From.Value}% to {range.To.Value}%",
                    (false, true) => $"over {range.From.Value}%",
                    (true, false) => $"up to {range.To.Value}%",
                    _ => string.Empty
                };

                tags.Add(new SimilarSchoolsSelectedFilterTagViewModel($"{range.Name} {rangeText}".Trim(), baseUrl + queryString));
            }
        }

        return tags;
    }

    private static string BuildQueryStringWithout(
        Dictionary<string, List<string>> currentFilters,
        IEnumerable<(string Key, string Value)> exclude)
    {
        var parts = new List<string>();

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

}

public record PrimarySimilarSchoolsRowViewModel(
    string Urn,
    string Name,
    string LocalAuthorityName,
    string Rank,
    string Distance,
    string ComparisonUrl,
    PrimarySimilarSchoolsCharacteristicsViewModel Characteristics);

public record PrimarySimilarSchoolsCharacteristicsViewModel(
    string ReadMatAverage,
    string Ks1PriorRwmAverage,
    string PupilPremiumEligibilityPercentage,
    string PupilsWithEalPercentage,
    string Polar4Quintile,
    string PupilStabilityRate,
    string AverageIdaciScore,
    string PupilsWithSenSupportPercentage,
    string PupilCount,
    string PupilsWithEhcPlanPercentage)
{
    public static PrimarySimilarSchoolsCharacteristicsViewModel FromResponse(
        PrimarySimilarSchoolCharacteristics characteristics) =>
        new(
            DisplayDecimal(characteristics.ReadMatAverage),
            DisplayDecimal(characteristics.Ks1PriorRwmAverage),
            DisplayPercent(characteristics.PupilPremiumEligibilityPercentage),
            DisplayPercent(characteristics.PupilsWithEalPercentage),
            DisplayDecimal(characteristics.Polar4Quintile),
            DisplayPercent(characteristics.PupilStabilityRate),
            DisplayDecimal(characteristics.AverageIdaciScore, "0.###"),
            DisplayPercent(characteristics.PupilsWithSenSupportPercentage),
            DisplayDecimal(characteristics.PupilCount, "0"),
            DisplayPercent(characteristics.PupilsWithEhcPlanPercentage));

    private static string DisplayDecimal(decimal value, string format = "0.#") =>
        value.ToString(format, CultureInfo.InvariantCulture);

    private static string DisplayPercent(decimal value) =>
        value.ToString("0.#", CultureInfo.InvariantCulture) + "%";
}
