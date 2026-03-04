namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetSimilarSchoolsSecondaryNationalStandardDeviations(ISimilarSchoolsSecondaryRepository repository)
{
    public async Task<GetSimilarSchoolsSecondaryNationalStandardDeviationsResponse> Execute(GetSimilarSchoolsSecondaryNationalStandardDeviationsRequest request)
    {
        var sd = await repository.GetSimilarSchoolsSecondaryNationalSdAsync();
        return new GetSimilarSchoolsSecondaryNationalStandardDeviationsResponse(sd);
    }
}

public record GetSimilarSchoolsSecondaryNationalStandardDeviationsRequest;

public record GetSimilarSchoolsSecondaryNationalStandardDeviationsResponse(SimilarSchoolsSecondaryNationalSD SimilarSchoolsSecondaryNationalSD);
