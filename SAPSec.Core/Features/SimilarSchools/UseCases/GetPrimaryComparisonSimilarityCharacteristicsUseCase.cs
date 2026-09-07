using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetPrimaryComparisonSimilarityCharacteristicsUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository)
    : IUseCase<GetPrimaryComparisonSimilarityCharacteristicsRequest, GetPrimaryComparisonSimilarityCharacteristicsResponse>
{
    public async Task<GetPrimaryComparisonSimilarityCharacteristicsResponse> Execute(GetPrimaryComparisonSimilarityCharacteristicsRequest request)
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

        var values = SimilarSchoolsPrimaryValues.FromData(await similarSchoolsRepository.GetValuesByUrnsAsync(urns))
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
            PrimaryComparisonSimilarityCharacteristics.Build(currentSchoolValues, comparatorSchoolValues));
    }
}

public record GetPrimaryComparisonSimilarityCharacteristicsRequest(
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn);

public record GetPrimaryComparisonSimilarityCharacteristicsResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo ComparatorSchool,
    PrimaryComparisonSimilarityCharacteristics SimilarityCharacteristics);
