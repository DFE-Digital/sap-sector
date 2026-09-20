using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Secondary;

public class GetComparisonKs4CoreSubjectsMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4PerformanceRepository performanceRepository)
    : IUseCase<GetComparisonKs4CoreSubjectsMeasuresRequest, GetComparisonKs4CoreSubjectsMeasuresResponse>
{
    public async Task<GetComparisonKs4CoreSubjectsMeasuresResponse> Execute(GetComparisonKs4CoreSubjectsMeasuresRequest request)
    {
        var performance = new ComparisonMeasureDataProvider<Ks4PerformanceData, SimilarSchoolsSecondaryGroupsEntry, SimilarSchoolsSecondaryValuesEntry>(
            establishmentRepository,
            similarSchoolsRepository,
            performanceRepository);

        var (currentSchoolPerformance, comparatorSchoolPerformance) = await performance.GetData(request.CurrentSchoolUrn, request.ComparatorSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            comparatorSchoolPerformance.SchoolInfo,
            Ks4CoreSubjects.EnglishLanguage.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.EnglishLiterature.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.Maths.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.CombinedScience.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.Biology.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.Chemistry.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy),
            Ks4CoreSubjects.Physics.ForSchoolComparison(
                currentSchoolPerformance,
                comparatorSchoolPerformance,
                filterBy)
        );
    }
}

public record GetComparisonKs4CoreSubjectsMeasuresRequest(
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonKs4CoreSubjectsMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo ComparatorSchool,
    Measure EnglishLanguage,
    Measure EnglishLiterature,
    Measure Maths,
    Measure CombinedScience,
    Measure Biology,
    Measure Chemistry,
    Measure Physics
);
