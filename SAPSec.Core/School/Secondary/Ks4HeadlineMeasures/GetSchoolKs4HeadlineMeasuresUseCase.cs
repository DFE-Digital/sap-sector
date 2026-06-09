using SAPSec.Core.Measures;
using SAPSec.Core.School.Info;
using SAPSec.Core.UseCases;

namespace SAPSec.Core.School.Secondary.Ks4HeadlineMeasures;

public class GetSchoolKs4HeadlineMeasuresUseCase(
    ISecondarySchoolsRepository repository)
    : IUseCase<GetSchoolKs4HeadlineMeasuresRequest, GetSchoolKs4HeadlineMeasuresResponse>
{
    public async Task<GetSchoolKs4HeadlineMeasuresResponse> Execute(GetSchoolKs4HeadlineMeasuresRequest request)
    {
        var (currentSchoolPerformance, similarSchoolsPerformance) = await repository.GetSimilarSchoolsPerformance(request.Urn);
        var (currentSchoolDestinations, similarSchoolsDestinations) = await repository.GetSimilarSchoolsDestinations(request.Urn);

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolsPerformance.Count,
            Ks4HeadlineMeasures.Attainment8.ForSecondarySchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4HeadlineMeasures.EnglishAndMaths.ForSecondarySchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4HeadlineMeasures.Destinations.ForSecondarySchool(
                currentSchoolDestinations,
                similarSchoolsDestinations,
                filterBy));
    }
}

public record GetSchoolKs4HeadlineMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4HeadlineMeasuresResponse(
    SchoolInfo School,
    int SimilarSchoolsCount,
    Measure Attainment8,
    Measure EnglishAndMaths,
    Measure Destinations);