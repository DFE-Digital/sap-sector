namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public class GetFilteredSchoolKs4CoreSubject(GetSchoolKs4CoreSubjects getSchoolKs4CoreSubjects)
{
    public async Task<GetFilteredSchoolKs4CoreSubjectResponse> Execute(GetFilteredSchoolKs4CoreSubjectRequest request)
    {
        var gradeFilter = ParseGradeFilter(request.Grade);
        var subjectFilter = ParseSubject(request.Subject);
        var response = await getSchoolKs4CoreSubjects.Execute(new GetSchoolKs4CoreSubjectsRequest(request.Urn));

        return new(
            SchoolKs4CoreSubjectSelection.From(response, subjectFilter, gradeFilter),
            subjectFilter,
            gradeFilter);
    }

    private static SchoolKs4CoreSubjectGradeFilter ParseGradeFilter(string? grade) =>
        grade switch
        {
            "5" => SchoolKs4CoreSubjectGradeFilter.Grade5,
            "7" => SchoolKs4CoreSubjectGradeFilter.Grade7,
            _ => SchoolKs4CoreSubjectGradeFilter.Grade4
        };

    private static SchoolKs4CoreSubject ParseSubject(string? subject) =>
        subject?.ToLowerInvariant() switch
        {
            "english-literature" => SchoolKs4CoreSubject.EnglishLiterature,
            "biology" => SchoolKs4CoreSubject.Biology,
            "chemistry" => SchoolKs4CoreSubject.Chemistry,
            "physics" => SchoolKs4CoreSubject.Physics,
            "maths" => SchoolKs4CoreSubject.Maths,
            "combined-science-double-award" => SchoolKs4CoreSubject.CombinedScienceDoubleAward,
            _ => SchoolKs4CoreSubject.EnglishLanguage
        };

}

public record GetFilteredSchoolKs4CoreSubjectRequest(
    string Urn,
    string? Subject,
    string? Grade);

public record GetFilteredSchoolKs4CoreSubjectResponse(
    SchoolKs4CoreSubjectSelection Selection,
    SchoolKs4CoreSubject Subject,
    SchoolKs4CoreSubjectGradeFilter Grade);
