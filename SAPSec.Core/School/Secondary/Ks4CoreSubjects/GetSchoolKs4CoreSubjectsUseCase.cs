using SAPSec.Core.Measures;
using SAPSec.Core.School.Info;
using SAPSec.Core.UseCases;

namespace SAPSec.Core.School.Secondary.Ks4CoreSubjects;

public class GetSchoolKs4CoreSubjectsUseCase(
    ISecondarySchoolsRepository repository)
    : IUseCase<GetSchoolKs4CoreSubjectsRequest, GetSchoolKs4CoreSubjectsResponse>
{
    public async Task<GetSchoolKs4CoreSubjectsResponse> Execute(GetSchoolKs4CoreSubjectsRequest request)
    {
        var (currentSchool, similarSchools) = await repository.GetSimilarSchoolsPerformance(request.Urn);

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchool.SchoolInfo,
            similarSchools.Count,
            [
                Ks4CoreSubjects.EnglishLanguage.ForSecondarySchool(currentSchool, similarSchools, filterBy),
                Ks4CoreSubjects.EnglishLiterature.ForSecondarySchool(currentSchool, similarSchools, filterBy),
                Ks4CoreSubjects.Biology.ForSecondarySchool(currentSchool, similarSchools, filterBy),
                Ks4CoreSubjects.Chemistry.ForSecondarySchool(currentSchool, similarSchools, filterBy),
                Ks4CoreSubjects.Physics.ForSecondarySchool(currentSchool, similarSchools, filterBy),
                Ks4CoreSubjects.Mathematics.ForSecondarySchool(currentSchool, similarSchools, filterBy),
                Ks4CoreSubjects.CombinedScience.ForSecondarySchool(currentSchool, similarSchools, filterBy),
            ]);
    }
}

public record GetSchoolKs4CoreSubjectsRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4CoreSubjectsResponse(
    SchoolInfo School,
    int SimilarSchoolsCount,
    IReadOnlyCollection<Measure> Measures);
