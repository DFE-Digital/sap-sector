using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public class GetSchoolComparisonKs4CoreSubjectsUseCase(
    IEstablishmentRepository establishmentRepository,
    IKs4PerformanceRepository performanceRepository)
    : IUseCase<GetSchoolComparisonKs4CoreSubjectsRequest, GetSchoolComparisonKs4CoreSubjectsResponse>
{
    public async Task<GetSchoolComparisonKs4CoreSubjectsResponse> Execute(GetSchoolComparisonKs4CoreSubjectsRequest request)
    {
        var performance = new SecondarySchoolComparisonPerformanceDataProvider(
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

public record GetSchoolComparisonKs4CoreSubjectsRequest(
    string Urn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolComparisonKs4CoreSubjectsResponse(
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
