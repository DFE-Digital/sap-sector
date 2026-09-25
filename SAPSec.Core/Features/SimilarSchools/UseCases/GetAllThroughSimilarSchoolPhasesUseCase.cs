using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetAllThroughSimilarSchoolPhasesUseCase(
    ISimilarSchoolsPrimaryRepository similarSchoolsPrimaryRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsSecondaryRepository)
    : IUseCase<GetAllThroughSimilarSchoolPhasesRequest, GetAllThroughSimilarSchoolPhasesResponse>
{
    public async Task<GetAllThroughSimilarSchoolPhasesResponse> Execute(GetAllThroughSimilarSchoolPhasesRequest request)
    {
        var primaryGroup = await similarSchoolsPrimaryRepository.GetGroupAsync(request.Urn);
        var secondaryGroup = await similarSchoolsSecondaryRepository.GetGroupAsync(request.Urn);

        return new(primaryGroup.Any(), secondaryGroup.Any());
    }
}

public record GetAllThroughSimilarSchoolPhasesRequest(string Urn);

public record GetAllThroughSimilarSchoolPhasesResponse(
    bool HasPrimary,
    bool HasSecondary);
