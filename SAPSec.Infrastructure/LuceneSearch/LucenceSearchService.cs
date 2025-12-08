using System.Text.RegularExpressions;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Search;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.Interfaces;
using SAPSec.Infrastructure.LuceneSearch.Interfaces;

namespace SAPSec.Infrastructure.LuceneSearch;

public class LuceneSearchService(ILuceneIndexReader indexReader, IEstablishmentService _establishmentService) : ISearchRepository
{
    public async Task<IReadOnlyList<EstablishmentSearchResult>> SearchAsync(string query)
    {
        var searchResults = await indexReader.SearchAsync(query);

        var results = new List<EstablishmentSearchResult>();

        if (searchResults.Count == 0) return results;

        foreach (var (urn, schoolName) in searchResults)
        {
            var school = _establishmentService.GetEstablishment(urn.ToString());
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
