using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class GetSchoolKs2PerformanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository,
    IKs2PerformanceRepository performanceRepository)
    : IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse>
{
    public async Task<GetSchoolKs2PerformanceMeasuresResponse> Execute(GetSchoolKs2PerformanceMeasuresRequest request)
    {
        var dataProvider = new PrimarySimilarSchoolsPerformanceDataProvider(
            establishmentRepository,
            similarSchoolsRepository,
            performanceRepository);

        var (currentSchoolPerformance, similarSchoolsPerformance) = await dataProvider.GetSimilarSchoolsPerformance(request.Urn);

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolsPerformance.Count,
            Ks2PerformanceMeasures.MeetingExpectedStandardRwm.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy
            ),
            Ks2PerformanceMeasures.AverageScaledScoreReading.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy
            )
        );
    }
}

public record GetSchoolKs2PerformanceMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs2PerformanceMeasuresResponse(
    SchoolInfo.SchoolInfo School,
    int SimilarSchoolsCount,
    Measure MeetingExpectedStandardRwm,
    Measure AverageScaledScoreReading);
