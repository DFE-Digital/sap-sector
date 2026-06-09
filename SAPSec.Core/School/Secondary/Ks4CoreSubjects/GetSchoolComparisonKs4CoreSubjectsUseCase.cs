using SAPSec.Core.Exceptions;
using SAPSec.Core.Measures;
using SAPSec.Core.School.Info;
using SAPSec.Core.UseCases;

namespace SAPSec.Core.School.Secondary.Ks4CoreSubjects;

public class GetSchoolComparisonKs4CoreSubjectsUseCase(
    ISecondarySchoolsRepository repository)
    : IUseCase<GetSchoolComparisonKs4CoreSubjectsRequest, GetSchoolComparisonKs4CoreSubjectsResponse>
{
    public async Task<GetSchoolComparisonKs4CoreSubjectsResponse> Execute(GetSchoolComparisonKs4CoreSubjectsRequest request)
    {
        var (currentSchool, similarSchools) = await repository.GetSimilarSchoolsPerformance(request.CurrentSchoolUrn);

        var similarSchool = similarSchools.FirstOrDefault(s => s.SchoolInfo.Urn == request.SimilarSchoolUrn);
        if (similarSchool is null)
        {
            throw new NotFoundException($"School not found with URN: {request.SimilarSchoolUrn}");
        }

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchool.SchoolInfo,
            similarSchool.SchoolInfo,
            similarSchools.Count,
            [
                Ks4CoreSubjects.EnglishLanguage.ForSecondarySchoolComparison(currentSchool, similarSchool, similarSchools, filterBy),
                Ks4CoreSubjects.EnglishLiterature.ForSecondarySchoolComparison(currentSchool, similarSchool, similarSchools, filterBy),
                Ks4CoreSubjects.Biology.ForSecondarySchoolComparison(currentSchool, similarSchool, similarSchools, filterBy),
                Ks4CoreSubjects.Chemistry.ForSecondarySchoolComparison(currentSchool, similarSchool, similarSchools, filterBy),
                Ks4CoreSubjects.Physics.ForSecondarySchoolComparison(currentSchool, similarSchool, similarSchools, filterBy),
                Ks4CoreSubjects.Mathematics.ForSecondarySchoolComparison(currentSchool, similarSchool, similarSchools, filterBy),
                Ks4CoreSubjects.CombinedScience.ForSecondarySchoolComparison(currentSchool, similarSchool, similarSchools, filterBy),
            ]);
    }
}

public record GetSchoolComparisonKs4CoreSubjectsRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolComparisonKs4CoreSubjectsResponse(
    SchoolInfo CurrentSchool,
    SchoolInfo SimilarSchool,
    int SimilarSchoolsCount,
    IReadOnlyCollection<Measure> Measures);
