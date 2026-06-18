using SAPSec.Core.UseCases;

namespace SAPSec.Core.School.Search;

public class FindASchoolSuggestionsUseCase(
    ISchoolSearchService searchService)
    : IUseCase<GetSearchSuggestionsRequest, GetSearchSuggestionsResponse>
{
    public Task<GetSearchSuggestionsResponse> Execute(GetSearchSuggestionsRequest request)
    {
        throw new NotImplementedException();
    }
}

public record GetSearchSuggestionsRequest();

public record GetSearchSuggestionsResponse();