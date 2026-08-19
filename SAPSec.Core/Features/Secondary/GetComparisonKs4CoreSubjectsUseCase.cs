using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public class GetComparisonKs4CoreSubjectsUseCase(
    IEstablishmentRepository establishmentRepository,
    IKs4PerformanceRepository performanceRepository)
    : IUseCase<GetComparisonKs4CoreSubjectsRequest, GetComparisonKs4CoreSubjectsResponse>
{
    public async Task<GetComparisonKs4CoreSubjectsResponse> Execute(GetComparisonKs4CoreSubjectsRequest request)
    {
        var performance = new ComparisonPerformanceDataProvider(
            establishmentRepository,
            performanceRepository);

        var (currentSchoolPerformance, similarSchoolPerformance) = await performance.GetData(request.Urn, request.SimilarSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolPerformance.SchoolInfo,
            Ks4CoreSubjects.EnglishLanguage.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.EnglishLiterature.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.Maths.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.CombinedScience.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.Biology.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.Chemistry.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.Physics.ForSchoolComparison(
                currentSchoolPerformance,
                similarSchoolPerformance,
                filterBy)
        );
    }
}

public record GetComparisonKs4CoreSubjectsRequest(
    string Urn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonKs4CoreSubjectsResponse(
    SchoolInfo.SchoolInfo School,
    SchoolInfo.SchoolInfo SimilarSchool,
    Measure EnglishLanguage,
    Measure EnglishLiterature,
    Measure Maths,
    Measure CombinedScience,
    Measure Biology,
    Measure Chemistry,
    Measure Physics
);
