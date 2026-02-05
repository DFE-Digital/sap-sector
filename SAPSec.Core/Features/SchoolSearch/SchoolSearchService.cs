using Lucene.Net.Search;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SAPSec.Core.Features.SchoolSearch
{
    public class SchoolSearchService(ISchoolSearchIndexReader indexReader, IEstablishmentService _establishmentService) : ISchoolSearchService
    {
        private const int MaxResults = 1000;
        private const int MaxSuggestions = 10;

        public async Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query)
        {
            var searchResults = await indexReader.SearchAsync(query, MaxResults);

            var results = new List<SchoolSearchResult>();

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

                results.Add(SchoolSearchResult.FromNameAndEstablishment(schoolName, school));
            }

            return results;
        }

        public async Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart)
        {
            var searchResults = await indexReader.SearchAsync(queryPart, MaxSuggestions);

            var results = new List<SchoolSearchResult>();

            if (searchResults.Count == 0) return results;

            foreach (var (urn, schoolName) in searchResults)
            {
                var school = _establishmentService.GetEstablishment(urn.ToString());

                results.Add(SchoolSearchResult.FromNameAndEstablishment(schoolName, school));
            }

            return results;
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
}
