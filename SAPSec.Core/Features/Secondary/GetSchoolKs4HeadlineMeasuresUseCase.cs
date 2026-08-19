using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public class GetSchoolKs4HeadlineMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository)
    : IUseCase<GetSchoolKs4HeadlineMeasuresRequest, GetSchoolKs4HeadlineMeasuresResponse>
{
    public async Task<GetSchoolKs4HeadlineMeasuresResponse> Execute(GetSchoolKs4HeadlineMeasuresRequest request)
    {
        var performance = new SecondarySimilarSchoolsPerformanceDataProvider(
            establishmentRepository,
            similarSchoolsRepository,
            performanceRepository);

        var destinations = new SecondarySimilarSchoolsDestinationsDataProvider(
            establishmentRepository,
            similarSchoolsRepository,
            destinationsRepository);

        var (currentSchoolPerformance, similarSchoolsPerformance) = await performance.GetData(request.Urn);
        var (currentSchoolDestinations, similarSchoolsDestinations) = await destinations.GetData(request.Urn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolsPerformance.Count,
            Ks4HeadlineMeasures.Attainment8.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy
            ),
            Ks4HeadlineMeasures.EnglishMaths.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy
            ),
            Ks4HeadlineMeasures.Destinations.ForSchool(
                currentSchoolDestinations,
                similarSchoolsDestinations,
                filterBy
            )
        );
    }
}

public record GetSchoolKs4HeadlineMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4HeadlineMeasuresResponse(
    SchoolInfo.SchoolInfo School,
    int SimilarSchoolsCount,
    Measure Attainment8,
    Measure EnglishMaths,
    Measure Destinations);
