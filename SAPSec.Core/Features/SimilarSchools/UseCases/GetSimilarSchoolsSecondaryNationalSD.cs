namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetSimilarSchoolsSecondaryNationalSD(ISimilarSchoolsSecondaryRepository repository)
{
    public async Task<GetSimilarSchoolsSecondaryNationalSDResponse> Execute(GetSimilarSchoolsSecondaryNationalSDRequest request)
    {
        var sd = await repository.GetSimilarSchoolsSecondaryNationalSdAsync();
        return new GetSimilarSchoolsSecondaryNationalSDResponse(sd);
    }
}

public record GetSimilarSchoolsSecondaryNationalSDRequest;

public record GetSimilarSchoolsSecondaryNationalSDResponse(SimilarSchoolsSecondaryNationalSD SimilarSchoolsSecondaryNationalSD);
