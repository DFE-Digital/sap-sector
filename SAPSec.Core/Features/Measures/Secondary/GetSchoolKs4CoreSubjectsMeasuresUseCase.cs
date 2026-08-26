using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Secondary;

public class GetSchoolKs4CoreSubjectsMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4PerformanceRepository performanceRepository)
    : IUseCase<GetSchoolKs4CoreSubjectsMeasuresRequest, GetSchoolKs4CoreSubjectsMeasuresResponse>
{
    public async Task<GetSchoolKs4CoreSubjectsMeasuresResponse> Execute(GetSchoolKs4CoreSubjectsMeasuresRequest request)
    {
        var performance = new SchoolKs4PerformanceDataProvider(
            establishmentRepository,
            similarSchoolsRepository,
            performanceRepository);

        var (currentSchoolPerformance, similarSchoolsPerformance) = await performance.GetData(request.Urn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            similarSchoolsPerformance.Count,
            Ks4CoreSubjects.EnglishLanguage.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4CoreSubjects.EnglishLiterature.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4CoreSubjects.Maths.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4CoreSubjects.CombinedScience.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4CoreSubjects.Biology.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4CoreSubjects.Chemistry.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy),
            Ks4CoreSubjects.Physics.ForSchool(
                currentSchoolPerformance,
                similarSchoolsPerformance,
                filterBy)
        );
    }
}

public record GetSchoolKs4CoreSubjectsMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4CoreSubjectsMeasuresResponse(
    SchoolInfo.SchoolInfo School,
    int SimilarSchoolsCount,
    Measure EnglishLanguage,
    Measure EnglishLiterature,
    Measure Maths,
    Measure CombinedScience,
    Measure Biology,
    Measure Chemistry,
    Measure Physics
);
