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
        var dataProvider = new ComparisonMeasureDataProvider<Ks2PerformanceData>(
            establishmentRepository,
            performanceRepository);

        var (currentSchoolData, comparatorSchoolData) = await dataProvider.GetData(
            request.CurrentSchoolUrn,
            request.ComparatorSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolData.SchoolInfo,
            comparatorSchoolData.SchoolInfo,
            Ks2PerformanceMeasures.MeetingExpectedStandardRwm.ForSchoolComparison(
                currentSchoolData,
                comparatorSchoolData,
                filterBy),
            Ks2PerformanceMeasures.AchievedHigherStandardRwm.ForSchoolComparison(
                currentSchoolData,
                comparatorSchoolData,
                filterBy),
            Ks2PerformanceMeasures.AverageScaledScoreReading.ForSchoolComparison(
                currentSchoolData,
                comparatorSchoolData,
                filterBy),
            Ks2PerformanceMeasures.AverageScaledScoreMaths.ForSchoolComparison(
                currentSchoolData,
                comparatorSchoolData,
                filterBy),
            Ks2PerformanceMeasures.MeetingExpectedStandardGps.ForSchoolComparison(
                currentSchoolData,
                comparatorSchoolData,
                filterBy),
            Ks2PerformanceMeasures.AchievedHigherStandardGps.ForSchoolComparison(
                currentSchoolData,
                comparatorSchoolData,
                filterBy));
    }
}

public record GetComparisonKs2PerformanceMeasuresRequest(
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonKs2PerformanceMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo ComparatorSchool,
    Measure MeetingExpectedStandardRwm,
    Measure AchievedHigherStandardRwm,
    Measure AverageScaledScoreReading,
    Measure AverageScaledScoreMaths,
    Measure MeetingExpectedStandardGps,
    Measure AchievedHigherStandardGps);
