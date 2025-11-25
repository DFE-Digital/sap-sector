using System.Text.RegularExpressions;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Interfaces.Services.Lucene;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.Interfaces;

namespace SAPSec.Core.Services;

public class SearchService(ILuceneIndexReader indexReader, ISchoolRepository schoolRepository) : ISearchService
{
    public async Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query)
    {
        var searchResults = await indexReader.SearchAsync(query);

        var results = new List<SchoolSearchResult>();

        if (searchResults.Count == 0) return results;

        foreach (var (urn, schoolName) in searchResults)
        {
            var school = GetSchoolByUrnAsync(urn);
            results.Add(new SchoolSearchResult(schoolName, school));
        }

        return results;
    }

    public async Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart)
    {
        var result = await SearchAsync(queryPart);

        return result;
    }

    public School? SearchByNumber(string schoolNumber)
    {
        var isNumber = Regex.IsMatch(schoolNumber, @"^\d+$");
        var isDfENumber = Regex.IsMatch(schoolNumber, @"^\d+\\|/\d+$");

        return isNumber
            ? GetSchoolByNumberAsync(int.Parse(schoolNumber))
            : isDfENumber
                ? GetSchoolByNumberAsync(int.Parse(schoolNumber.Replace("/", string.Empty).Replace("\\", string.Empty)))
                : null;
    }

    public School GetSchoolByUrnAsync(int urn)
    {
        return schoolRepository.GetSchoolByUrn(urn);
    }

    private School? GetSchoolByNumberAsync(int schoolNumber)
    {
        return schoolRepository.GetSchoolByNumber(schoolNumber);
    }
}
