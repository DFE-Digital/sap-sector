using SAPSec.Core.Features.Geography;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.SchoolSearch;

public class SchoolSearchService(
    ISchoolSearchIndexReader _indexReader,
    IEstablishmentRepository _establishmentRepository) : ISchoolSearchService
{
    private const int MaxResults = 1000;
    private const int MaxSuggestions = 10;

    public async Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query)
    {
        var searchResults = await _indexReader.SearchAsync(query, MaxResults);

        var results = new List<SchoolSearchResult>();

        if (!searchResults.Any())
        {
            return results;
        }

        var schools = await _establishmentRepository.GetEstablishmentsAsync(searchResults.Select(r => r.urn.ToString()));

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

    public async Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart)
    {
        var searchResults = await _indexReader.SearchAsync(queryPart, MaxSuggestions);

        var results = new List<SchoolSearchResult>();

        if (!searchResults.Any())
        {
            return results;
        }

        var schools = await _establishmentRepository.GetEstablishmentsAsync(searchResults.Select(r => r.urn.ToString()));

        foreach (var r in searchResults.GroupJoin(schools,
            r => r.urn.ToString(),
            s => s.URN,
            (r, schools) => new { SchoolName = r.resultText, School = schools.FirstOrDefault() }))
        {
            if (r.School == null)
            {
                continue;
            }

            results.Add(SchoolSearchResult.FromNameAndEstablishment(r.SchoolName, r.School, null));
        }

        return results.OrderBy(r => r.EstablishmentName).ToList();
    }

    public async Task<Establishment?> SearchByNumberAsync(string schoolNumber)
    {
        var trimmedSchoolNumber = schoolNumber
            .Trim()
            .Replace("/", string.Empty)
            .Replace("\\", string.Empty);

        return await _establishmentRepository.GetEstablishmentByAnyNumberAsync(trimmedSchoolNumber);
    }
}
