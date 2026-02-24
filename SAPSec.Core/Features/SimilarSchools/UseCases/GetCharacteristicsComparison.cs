using SAPSec.Core.Features.SimilarSchools;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetCharacteristicsComparison(
    ISimilarSchoolsSecondaryRepository repository)
{
    public async Task<GetCharacteristicsComparisonResponse> Execute(GetCharacteristicsComparisonRequest request)
    {
        var urns = new[] { request.CurrentSchoolUrn, request.SimilarSchoolUrn };

        var values = await repository.GetSecondaryValuesByUrnsAsync(urns);

        if (!values.TryGetValue(request.CurrentSchoolUrn, out var current))
            throw new InvalidOperationException($"No characteristics found for URN {request.CurrentSchoolUrn}");

        if (!values.TryGetValue(request.SimilarSchoolUrn, out var similar))
            throw new InvalidOperationException($"No characteristics found for URN {request.SimilarSchoolUrn}");

        return new(current, similar);
    }
}

public record GetCharacteristicsComparisonRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn);

public record GetCharacteristicsComparisonResponse(
    SimilarSchoolsSecondaryValues CurrentSchool,
    SimilarSchoolsSecondaryValues SimilarSchool);
