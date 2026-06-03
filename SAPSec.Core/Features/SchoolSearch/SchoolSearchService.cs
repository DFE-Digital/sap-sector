using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SchoolSearch.Extensions;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;
using System.Text.RegularExpressions;

namespace SAPSec.Core.Features.SchoolSearch;

public class SchoolSearchService(
    ISchoolSearchIndexReader _indexReader,
    IEstablishmentRepository _establishmentRepository) : ISchoolSearchService
{
    private const int MaxResults = 1000;
    private const int MaxSuggestions = 10;
    private static readonly Regex Numeric = new Regex(@"^\d+$", RegexOptions.Compiled);

    public async Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query, bool primarySchoolsEnabled = false)
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

            if (!IsIncludedPhase(r.School.PhaseOfEducationName, primarySchoolsEnabled))
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

    public async Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart, bool primarySchoolsEnabled = false)
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

            if (!IsIncludedPhase(r.School.PhaseOfEducationName, primarySchoolsEnabled))
            {
                continue;
            }

            results.Add(SchoolSearchResult.FromNameAndEstablishment(r.SchoolName, r.School, null));
        }

        return results.OrderBy(r => r.EstablishmentName).ToList();
    }

    public async Task<Establishment?> SearchByNumberAsync(string schoolNumber, bool primarySchoolsEnabled = false)
    {
        var trimmedSchoolNumber = schoolNumber
            .Trim()
            .Replace("/", string.Empty)
            .Replace("\\", string.Empty);

        if (!Numeric.IsMatch(trimmedSchoolNumber))
        {
            return null;
        }

        var school = await _establishmentRepository.GetEstablishmentByAnyNumberAsync(trimmedSchoolNumber);

        return IsIncludedPhase(school?.PhaseOfEducationName, primarySchoolsEnabled)
            ? school
            : null;
    }

    private static bool IsIncludedPhase(string? phaseOfEducation, bool primarySchoolsEnabled)
    {
        if (string.IsNullOrWhiteSpace(phaseOfEducation))
        {
            return false;
        }

        return phaseOfEducation.Trim() switch
        {
            "Secondary" => true,
            "Primary" => primarySchoolsEnabled,
            "All-through" => primarySchoolsEnabled,
            _ => false
        };
    }
}
