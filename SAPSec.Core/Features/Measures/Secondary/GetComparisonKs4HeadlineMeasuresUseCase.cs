using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Secondary;

public class GetComparisonKs4HeadlineMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository)
    : IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse>
{
    public async Task<GetComparisonKs4HeadlineMeasuresResponse> Execute(GetComparisonKs4HeadlineMeasuresRequest request)
    {
        var performance = new ComparisonKs4PerformanceDataProvider(
            establishmentRepository,
            performanceRepository);

        var destinations = new ComparisonKs4DestinationsDataProvider(
            establishmentRepository,
            destinationsRepository);

        var (currentSchoolPerformance, similarSchoolPerformance) = await performance.GetData(request.CurrentSchoolUrn, request.SimilarSchoolUrn);
        var (currentSchoolDestinations, similarSchoolDestinations) = await destinations.GetData(request.CurrentSchoolUrn, request.SimilarSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolPerformance.SchoolInfo,
            Ks4HeadlineMeasures.Attainment8.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy
            ),
            Ks4HeadlineMeasures.EnglishMaths.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy
            ),
            Ks4HeadlineMeasures.Destinations.ForSchoolComparison(
                currentSchoolDestinations,
                similarSchoolDestinations,
                filterBy
            )
        );
    }
}

public record GetComparisonKs4HeadlineMeasuresRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonKs4HeadlineMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo SimilarSchool,
    Measure Attainment8,
    Measure EnglishMaths,
    Measure Destinations);
