using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Secondary;

public class GetComparisonKs4CoreSubjectsMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IKs4PerformanceRepository performanceRepository)
    : IUseCase<GetComparisonKs4CoreSubjectsMeasuresRequest, GetComparisonKs4CoreSubjectsMeasuresResponse>
{
    public async Task<GetComparisonKs4CoreSubjectsMeasuresResponse> Execute(GetComparisonKs4CoreSubjectsMeasuresRequest request)
    {
        var performance = new ComparisonMeasureDataProvider<Ks4PerformanceData>(
            establishmentRepository,
            performanceRepository);

        var (currentSchoolPerformance, similarSchoolPerformance) = await performance.GetData(request.CurrentSchoolUrn, request.SimilarSchoolUrn);

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

public record GetComparisonKs4CoreSubjectsMeasuresRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonKs4CoreSubjectsMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo SimilarSchool,
    Measure EnglishLanguage,
    Measure EnglishLiterature,
    Measure Maths,
    Measure CombinedScience,
    Measure Biology,
    Measure Chemistry,
    Measure Physics
);
