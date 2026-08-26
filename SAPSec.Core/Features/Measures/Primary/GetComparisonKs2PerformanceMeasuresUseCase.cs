using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Primary;

public class GetComparisonKs2PerformanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IKs2PerformanceRepository performanceRepository)
    : IUseCase<GetComparisonKs2PerformanceMeasuresRequest, GetComparisonKs2PerformanceMeasuresResponse>
{
    public async Task<GetComparisonKs2PerformanceMeasuresResponse> Execute(GetComparisonKs2PerformanceMeasuresRequest request)
    {
        var dataProvider = new ComparisonKs2PerformanceDataProvider(
            establishmentRepository,
            performanceRepository);

        var (currentSchoolData, similarSchoolData) = await dataProvider.GetData(
            request.CurrentSchoolUrn,
            request.SimilarSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolData.SchoolInfo,
            similarSchoolData.SchoolInfo,
            Ks2PerformanceMeasures.MeetingExpectedStandardRwm.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                filterBy),
            Ks2PerformanceMeasures.AchievedHigherStandardRwm.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                filterBy),
            Ks2PerformanceMeasures.AverageScaledScoreReading.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                filterBy),
            Ks2PerformanceMeasures.AverageScaledScoreMaths.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                filterBy),
            Ks2PerformanceMeasures.MeetingExpectedStandardGps.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                filterBy),
            Ks2PerformanceMeasures.AchievedHigherStandardGps.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                filterBy));
    }
}

public record GetComparisonKs2PerformanceMeasuresRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonKs2PerformanceMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo SimilarSchool,
    Measure MeetingExpectedStandardRwm,
    Measure AchievedHigherStandardRwm,
    Measure AverageScaledScoreReading,
    Measure AverageScaledScoreMaths,
    Measure MeetingExpectedStandardGps,
    Measure AchievedHigherStandardGps);
