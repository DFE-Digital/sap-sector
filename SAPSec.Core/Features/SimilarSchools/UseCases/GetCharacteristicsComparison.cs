using SAPSec.Core.Features.SimilarSchools;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetCharacteristicsComparison(
    ISimilarSchoolsSecondaryRepository repository)
{
    public async Task<GetCharacteristicsComparisonResponse> Execute(GetCharacteristicsComparisonRequest request)
    {
        var urns = new[] { request.CurrentSchoolUrn, request.SimilarSchoolUrn };

        var values = await repository.GetSecondaryValuesByUrnsAsync(urns);

        var current = values.FirstOrDefault(v => v.Urn == request.CurrentSchoolUrn);
        if (current is null)
            throw new NotFoundException($"No characteristics found for URN {request.CurrentSchoolUrn}");

        var similar = values.FirstOrDefault(v => v.Urn == request.SimilarSchoolUrn);
        if (similar is null)
            throw new NotFoundException($"No characteristics found for URN {request.SimilarSchoolUrn}");

        return new(current, similar);
    }
}

public record GetCharacteristicsComparisonRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn);

public record GetCharacteristicsComparisonResponse(
    SimilarSchoolsSecondaryValues CurrentSchool,
    SimilarSchoolsSecondaryValues SimilarSchool);
