using SAPSec.Core.UseCases;

namespace SAPSec.Core.School.Search;

public class GetSearchSuggestionsUseCase(
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