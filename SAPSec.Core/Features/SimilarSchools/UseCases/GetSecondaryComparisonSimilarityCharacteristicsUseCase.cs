using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetSecondaryComparisonSimilarityCharacteristicsUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
    : IUseCase<GetSecondaryComparisonSimilarityCharacteristicsRequest, GetSecondaryComparisonSimilarityCharacteristicsResponse>
{
    public async Task<GetSecondaryComparisonSimilarityCharacteristicsResponse> Execute(GetSecondaryComparisonSimilarityCharacteristicsRequest request)
    {
        string[] urns = [request.CurrentSchoolUrn, request.ComparatorSchoolUrn];

        var schools = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(s => s.Urn);

        if (!schools.TryGetValue(request.CurrentSchoolUrn, out var currentSchool))
        {
            throw new NotFoundException($"No school found with URN {request.CurrentSchoolUrn}");
        }

        if (!schools.TryGetValue(request.ComparatorSchoolUrn, out var comparatorSchool))
        {
            throw new NotFoundException($"No school found with URN {request.ComparatorSchoolUrn}");
        }

        var values = SimilarSchoolsSecondaryValues.FromData(await similarSchoolsRepository.GetValuesByUrnsAsync(urns))
            .ToDictionary(s => s.Urn);

        if (!values.TryGetValue(request.CurrentSchoolUrn, out var currentSchoolValues))
        {
            throw new NotFoundException($"No characteristics found for URN {request.CurrentSchoolUrn}");
        }

        if (!values.TryGetValue(request.ComparatorSchoolUrn, out var comparatorSchoolValues))
        {
            throw new NotFoundException($"No characteristics found for URN {request.ComparatorSchoolUrn}");
        }

        return new(
            currentSchool,
            comparatorSchool,
            SecondaryComparisonSimilarityCharacteristics.Build(currentSchoolValues, comparatorSchoolValues));
    }
}

public record GetSecondaryComparisonSimilarityCharacteristicsRequest(
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn);

public record GetSecondaryComparisonSimilarityCharacteristicsResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo ComparatorSchool,
    SecondaryComparisonSimilarityCharacteristics SimilarityCharacteristics);
