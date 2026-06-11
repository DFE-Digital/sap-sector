using SAPSec.Core.Filtering;
using SAPSec.Core.Geography;
using SAPSec.Core.Pagination;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;
using System.Text.RegularExpressions;

namespace SAPSec.Core.School.Search;

public class GetSearchResultsUseCase(
    IEstablishmentStore establishmentStore,
    ISchoolSearchIndexReader indexReader)
    : IUseCase<GetSearchResultsRequest, GetSearchResultsResponse>
{
    private const int PageSize = 10;
    private const int MaxResults = 1000;
    private const int MaxSuggestions = 10;
    private static readonly Regex Numeric = new Regex(@"^\d+$", RegexOptions.Compiled);

    public async Task<GetSearchResultsResponse> Execute(GetSearchResultsRequest request)
    {
        var page = Math.Max(1, request.Page ?? 1);
        var query = request.Query.Trim();
        var filterBy = request.FilterBy ?? new Dictionary<string, IEnumerable<string>>();
        var localAuthorities = filterBy.ContainsKey("la") ? filterBy["la"] : [];

        List<SchoolSearchResult> results = [];

        if (IsSchoolNumberCandidate(query))
        {
            var school = await SearchByNumberAsync(query);
            if (school is not null)
            {
                var latLong = BNGCoordinates.TryParse(school.Easting, school.Northing, out var coords)
                    ? CoordinateConverter.Convert(coords)
                    : null;

                results.Add(SchoolSearchResult.FromNameAndEstablishment(school.EstablishmentName, school, latLong));
            }
        }
        else
        {
            results = (await SearchAsync(query)).ToList();

            // Preserve direct navigation when the query uniquely matches a school name,
            // even if hidden filters would later reduce results to zero.
            if (!string.IsNullOrWhiteSpace(query) && !localAuthorities.Any())
            {
                var exactMatches = results
                    .Where(s => string.Equals(
                        s.EstablishmentName?.Trim(),
                        query,
                        StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

                if (exactMatches.Count == 1)
                {
                    results = exactMatches;
                }
            }
            else if (localAuthorities.Any())
            {
                results = results
                    .Where(s => localAuthorities.Contains(s.LANAme))
                    .ToList();

                //if (results.Count == 1 && (localAuthorities == null || localAuthorities.Length == 0))
                //{
                //    return RedirectToAction("Index", "School", new { results[0].URN });
                //}
            }
        }

        var pagedResults = new PagedCollection<SchoolSearchResult>(results, page, request.ResultsPerPage);

        var laFilter = new SchoolSearchAvailableFilter(
            "la",
            "Local authority",
            results
                .Select(s => s.LANAme)
                .Where(la => !string.IsNullOrWhiteSpace(la))
                .Distinct()
                .OrderBy(la => la)
                .Select(la => new FilterOption(la, la, 0, localAuthorities.Contains(la)))
                .ToArray());

        return new([laFilter], pagedResults, results);
    }

    public async Task<Establishment?> SearchByNumberAsync(string schoolNumber)
    {
        var trimmedSchoolNumber = schoolNumber
            .Trim()
            .Replace("/", string.Empty)
            .Replace("\\", string.Empty);

        if (!Numeric.IsMatch(trimmedSchoolNumber))
        {
            return null;
        }

        var establishment = await establishmentStore.GetEstablishmentByAnyNumberAsync(trimmedSchoolNumber);
        return establishment.IsSearchable() ? establishment : null;
    }

    public async Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query)
    {
        var searchResults = await indexReader.SearchAsync(query, MaxResults);

        var results = new List<SchoolSearchResult>();

        if (!searchResults.Any())
        {
            return results;
        }

        var schools = await establishmentStore.GetEstablishmentsAsync(searchResults.Select(r => r.urn.ToString()));

        foreach (var r in searchResults.GroupJoin(schools,
            r => r.urn.ToString(),
            s => s.URN,
            (r, schools) => new { SchoolName = r.resultText, School = schools.FirstOrDefault() }))
        {
            if (r.School == null)
            {
                continue;
            }

            var latLong = BNGCoordinates.TryParse(r.School.Easting, r.School.Northing, out var coords)
                ? CoordinateConverter.Convert(coords)
                : null;

            results.Add(SchoolSearchResult.FromNameAndEstablishment(r.SchoolName, r.School, latLong));
        }

        return results.OrderBy(r => r.EstablishmentName).ToList();
    }

    private static bool IsSchoolNumberCandidate(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
            Regex.IsMatch(value.Trim(), @"^(\d+|\d+[\\/]\d+)$");

}

public record GetSearchResultsRequest(
    string Query,
    IDictionary<string, IEnumerable<string>>? FilterBy = null,
    int? Page = null,
    int ResultsPerPage = 10);

public record GetSearchResultsResponse(
    IReadOnlyCollection<SchoolSearchAvailableFilter> FilterOptions,
    IPagedCollection<SchoolSearchResult> ResultsPage,
    IReadOnlyCollection<SchoolSearchResult> AllResults);

public record SchoolSearchAvailableFilter(
    string Key,
    string Name,
    IReadOnlyCollection<FilterOption> Options);
