using SAPSec.Core.Interfaces.Services;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Integration.Tests.Mocks;

public class MockSearchService : ISearchService
{
    private readonly School _school102848 = new(
        urn: 102848,
        ukprn: 10000001,
        laCode: 123,
        establishmentNumber: 4567,
        establishmentName: "Test School 102848");

    private readonly School _anotherSchool = new(
        urn: 123456,
        ukprn: 10000002,
        laCode: 321,
        establishmentNumber: 7654,
        establishmentName: "Another School");

    public Task<IReadOnlyList<SchoolSearchResult>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<IReadOnlyList<SchoolSearchResult>>(
                Array.Empty<SchoolSearchResult>());
        }

        if (query.Contains("Saint Paul Roman Catholic", StringComparison.OrdinalIgnoreCase))
        {
            var saintPaul = new School(
                urn: 102848,
                ukprn: 10000003,
                laCode: 999,
                establishmentNumber: 1111,
                establishmentName: "Saint Paul Roman Catholic");

            return Task.FromResult<IReadOnlyList<SchoolSearchResult>>(
                new[]
                {
                    new SchoolSearchResult("Saint Paul Roman Catholic", saintPaul)
                });
        }

        if (query == "102848")
        {
            return Task.FromResult<IReadOnlyList<SchoolSearchResult>>(
                new[]
                {
                    new SchoolSearchResult("Test School 102848", _school102848)
                });
        }

        return Task.FromResult<IReadOnlyList<SchoolSearchResult>>(
            new[]
            {
                new SchoolSearchResult("Test School 102848", _school102848),
                new SchoolSearchResult("Another School", _anotherSchool)
            });
    }

    public Task<IReadOnlyList<SchoolSearchResult>> SuggestAsync(string queryPart)
    {
        if (string.IsNullOrWhiteSpace(queryPart))
        {
            return Task.FromResult<IReadOnlyList<SchoolSearchResult>>(
                Array.Empty<SchoolSearchResult>());
        }

        return Task.FromResult<IReadOnlyList<SchoolSearchResult>>(
            new[]
            {
                new SchoolSearchResult("Suggested School 102848", _school102848)
            });
    }

    public School? SearchByNumber(string schoolNumber)
    {
        return schoolNumber == "102848"
            ? _school102848
            : null;
    }

    public School GetSchoolByUrn(int urn)
    {
        if (urn == 102848)
            return _school102848;

        // if your controller doesn't null-check, you can return _anotherSchool instead:
        return null!;
    }
}
