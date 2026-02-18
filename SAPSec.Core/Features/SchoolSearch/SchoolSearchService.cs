using SAPSec.Core.Features.Geography;
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

            if (!searchResults.Any())
            {
                return results;
            }

            var schools = await _establishmentService.GetEstablishmentsAsync(searchResults.Select(r => r.urn.ToString()));

            foreach (var r in searchResults.GroupJoin(schools,
                r => r.urn.ToString(),
                s => s.URN,
                (r, schools) => new { SchoolName = r.resultText, School = schools.FirstOrDefault() }))
            {
                if (r.School == null)
                {
                    continue;
                }

                if (BNGCoordinates.TryParse(r.School.Easting, r.School.Northing, out var coords))
                {
                    var latLong = CoordinateConverter.Convert(coords);

                    r.School.Latitude = latLong.Latitude.ToString(CultureInfo.InvariantCulture);
                    r.School.Longitude = latLong.Longitude.ToString(CultureInfo.InvariantCulture);
                }

                results.Add(SchoolSearchResult.FromNameAndEstablishment(r.SchoolName, r.School));
            }

            return results;
        }

        public async Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart)
        {
            var searchResults = await indexReader.SearchAsync(queryPart, MaxSuggestions);

            var results = new List<SchoolSearchResult>();

            if (!searchResults.Any())
            {
                return results;
            }

            var schools = await _establishmentService.GetEstablishmentsAsync(searchResults.Select(r => r.urn.ToString()));

            foreach (var r in searchResults.GroupJoin(schools,
                r => r.urn.ToString(),
                s => s.URN,
                (r, schools) => new { SchoolName = r.resultText, School = schools.FirstOrDefault() }))
            {
                if (r.School == null)
                {
                    continue;
                }

                results.Add(SchoolSearchResult.FromNameAndEstablishment(r.SchoolName, r.School));
            }

            return results;
        }

        public async Task<Establishment?> SearchByNumberAsync(string schoolNumber)
        {
            var isNumber = Regex.IsMatch(schoolNumber, @"^\d+$");
            var isDfENumber = Regex.IsMatch(schoolNumber, @"^\d+\\|/\d+$");

            return isNumber
                ? (await _establishmentService.GetEstablishmentByAnyNumberAsync(schoolNumber))
                : isDfENumber
                    ? (await _establishmentService.GetEstablishmentByAnyNumberAsync(schoolNumber.Replace("/", string.Empty).Replace("\\", string.Empty)))
                    : null;
        }
    }
}
