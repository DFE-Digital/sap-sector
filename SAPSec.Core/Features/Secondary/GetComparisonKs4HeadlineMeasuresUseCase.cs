using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public class GetComparisonKs4HeadlineMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository)
    : IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse>
{
    public async Task<GetComparisonKs4HeadlineMeasuresResponse> Execute(GetComparisonKs4HeadlineMeasuresRequest request)
    {
        var performance = new ComparisonPerformanceDataProvider(
            establishmentRepository,
            performanceRepository);

        var destinations = new ComparisonDestinationsDataProvider(
            establishmentRepository,
            destinationsRepository);

        var (currentSchoolPerformance, similarSchoolPerformance) = await performance.GetData(request.Urn, request.SimilarSchoolUrn);
        var (currentSchoolDestinations, similarSchoolDestinations) = await destinations.GetData(request.Urn, request.SimilarSchoolUrn);

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
    string Urn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonKs4HeadlineMeasuresResponse(
    SchoolInfo.SchoolInfo School,
    SchoolInfo.SchoolInfo SimilarSchool,
    Measure Attainment8,
    Measure EnglishMaths,
    Measure Destinations);
