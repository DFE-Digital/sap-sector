using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Secondary;

public class GetComparisonKs4HeadlineMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository)
    : IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse>
{
    public async Task<GetComparisonKs4HeadlineMeasuresResponse> Execute(GetComparisonKs4HeadlineMeasuresRequest request)
    {
        var performance = new ComparisonMeasureDataProvider<Ks4PerformanceData, SimilarSchoolsSecondaryGroupsEntry, SimilarSchoolsSecondaryValuesEntry>(
            establishmentRepository,
            similarSchoolsRepository,
            performanceRepository);

        var destinations = new ComparisonMeasureDataProvider<Ks4DestinationsData, SimilarSchoolsSecondaryGroupsEntry, SimilarSchoolsSecondaryValuesEntry>(
            establishmentRepository,
            similarSchoolsRepository,
            destinationsRepository);

        var (currentSchoolPerformance, comparatorSchoolPerformance) = await performance.GetData(request.CurrentSchoolUrn, request.ComparatorSchoolUrn);
        var (currentSchoolDestinations, comparatorSchoolDestinations) = await destinations.GetData(request.CurrentSchoolUrn, request.ComparatorSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            comparatorSchoolPerformance.SchoolInfo,
            Ks4HeadlineMeasures.Attainment8.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy
            ),
            Ks4HeadlineMeasures.EnglishMaths.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy
            ),
            Ks4HeadlineMeasures.Destinations.ForSchoolComparison(
                currentSchoolDestinations,
                comparatorSchoolDestinations,
                filterBy
            )
        );
    }
}

public record GetComparisonKs4HeadlineMeasuresRequest(
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonKs4HeadlineMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo ComparatorSchool,
    Measure Attainment8,
    Measure EnglishMaths,
    Measure Destinations);
