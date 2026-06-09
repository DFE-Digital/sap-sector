using SAPSec.Core.Exceptions;
using SAPSec.Core.Measures;
using SAPSec.Core.School.Info;
using SAPSec.Core.UseCases;

namespace SAPSec.Core.School.Secondary.Ks4HeadlineMeasures;

public class GetSchoolComparisonKs4HeadlineMeasuresUseCase(
    ISecondarySchoolsRepository repository)
    : IUseCase<GetSchoolComparisonKs4HeadlineMeasuresRequest, GetSchoolComparisonKs4HeadlineMeasuresResponse>
{
    public async Task<GetSchoolComparisonKs4HeadlineMeasuresResponse> Execute(GetSchoolComparisonKs4HeadlineMeasuresRequest request)
    {
        var (currentSchoolPerformance, similarSchoolsPerformance) = await repository.GetSimilarSchoolsPerformance(request.CurrentSchoolUrn);
        var (currentSchoolDestinations, similarSchoolsDestinations) = await repository.GetSimilarSchoolsDestinations(request.SimilarSchoolUrn);

        var similarSchoolPerformance = similarSchoolsPerformance.FirstOrDefault(s => s.SchoolInfo.Urn == request.SimilarSchoolUrn);
        if (similarSchoolPerformance is null)
        {
            throw new NotFoundException($"School not found with URN: {request.SimilarSchoolUrn}");
        }

        var similarSchoolDestinations = similarSchoolsDestinations.FirstOrDefault(s => s.SchoolInfo.Urn == request.SimilarSchoolUrn);
        if (similarSchoolDestinations is null)
        {
            throw new NotFoundException($"School not found with URN: {request.SimilarSchoolUrn}");
        }

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolPerformance.SchoolInfo,
            similarSchoolsPerformance.Count,
            Ks4HeadlineMeasures.Attainment8.ForSecondarySchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4HeadlineMeasures.EnglishAndMaths.ForSecondarySchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4HeadlineMeasures.Destinations.ForSecondarySchoolComparison(
                currentSchoolDestinations,
                similarSchoolDestinations,
                similarSchoolsDestinations,
                filterBy));
    }
}

public record GetSchoolComparisonKs4HeadlineMeasuresRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolComparisonKs4HeadlineMeasuresResponse(
    SchoolInfo CurrentSchool,
    SchoolInfo SimilarSchool,
    int SimilarSchoolsCount,
    Measure Attainment8,
    Measure EnglishAndMaths,
    Measure Destinations);