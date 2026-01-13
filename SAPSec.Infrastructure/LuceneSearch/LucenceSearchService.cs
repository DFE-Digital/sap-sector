using System.Globalization;
using System.Text.RegularExpressions;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Search;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.Interfaces;
using SAPSec.Infrastructure.LuceneSearch.Interfaces;
using SAPSec.Infrastructure.Helper;

namespace SAPSec.Infrastructure.LuceneSearch;

public class LuceneSearchService(ILuceneIndexReader indexReader, IEstablishmentService _establishmentService) : ISearchRepository
{
    public async Task<IReadOnlyList<EstablishmentSearchResult>> SearchAsync(string query,int maxResults = 10)
    {
        var searchResults = await indexReader.SearchAsync(query, maxResults);

        var results = new List<EstablishmentSearchResult>();

        if (searchResults.Count == 0) return results;

        foreach (var (urn, schoolName) in searchResults)
        {
            var school = _establishmentService.GetEstablishment(urn.ToString());
            
            if (double.TryParse(school.Easting, NumberStyles.Any, CultureInfo.InvariantCulture, out var easting) &&
                double.TryParse(school.Northing, NumberStyles.Any, CultureInfo.InvariantCulture, out var northing))
            {
                var (lat, lon) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

                school.Latitude = lat.ToString(CultureInfo.InvariantCulture);
                school.Longitude = lon.ToString(CultureInfo.InvariantCulture);
            }
            
            results.Add(new EstablishmentSearchResult(schoolName, school));
        }

        return results;
    }

    public async Task<IReadOnlyList<EstablishmentSearchResult>> SuggestAsync(string queryPart)
    {
        var result = await SearchAsync(queryPart);

        return result;
    }

    public Establishment? SearchByNumber(string schoolNumber)
    {
        var isNumber = Regex.IsMatch(schoolNumber, @"^\d+$");
        var isDfENumber = Regex.IsMatch(schoolNumber, @"^\d+\\|/\d+$");

        return isNumber
            ? _establishmentService.GetEstablishmentByAnyNumber(schoolNumber)
            : isDfENumber
                ? _establishmentService.GetEstablishmentByAnyNumber(schoolNumber.Replace("/", string.Empty).Replace("\\", string.Empty))
                : null;
    }
}
