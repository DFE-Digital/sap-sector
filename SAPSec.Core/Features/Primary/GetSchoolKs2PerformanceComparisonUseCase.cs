using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class GetSchoolKs2PerformanceComparisonUseCase(
    IEstablishmentRepository establishmentRepository,
    IKs2PerformanceRepository performanceRepository)
    : IUseCase<GetSchoolKs2PerformanceComparisonRequest, GetSchoolKs2PerformanceComparisonResponse>
{
    public async Task<GetSchoolKs2PerformanceComparisonResponse> Execute(GetSchoolKs2PerformanceComparisonRequest request)
    {
        var dataProvider = new PrimarySchoolComparisonPerformanceDataProvider(
            establishmentRepository,
            performanceRepository);

        var (currentSchoolData, similarSchoolData) = await dataProvider.GetComparisonPerformance(
            request.Urn,
            request.SimilarSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            Ks2PerformanceMeasures.MeetingExpectedStandardRwm.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                [],
                filterBy),
            Ks2PerformanceMeasures.AchievedHigherStandardRwm.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                [],
                filterBy));
    }
}

public record GetSchoolKs2PerformanceComparisonRequest(
    string Urn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs2PerformanceComparisonResponse(
    Measure MeetingExpectedStandardRwm,
    Measure AchievedHigherStandardRwm);
