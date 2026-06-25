using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;

namespace SAPSec.Core.Features.Primary;

public class GetSchoolKs2PerformanceMeasuresUseCase(
    IPrimarySchoolsRepository repository)
    : IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse>
{
    public async Task<GetSchoolKs2PerformanceMeasuresResponse> Execute(GetSchoolKs2PerformanceMeasuresRequest request)
    {
        var (currentSchoolPerformance, similarSchoolsPerformance) = await repository.GetSimilarSchoolsPerformance(request.Urn);

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolsPerformance.Count,
            Ks2PerformanceMeasures.MeetingExpectedStandardRwm.ForSecondarySchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy));
    }
}

public record GetSchoolKs2PerformanceMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs2PerformanceMeasuresResponse(
    SchoolInfo.SchoolInfo School,
    int SimilarSchoolsCount,
    Measure MeetingExpectedStandardRwm);