namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public static class SchoolKs4CoreSubjectExtensions
{
    public static string ToFilterValue(this SchoolKs4CoreSubjectGradeFilter filter) =>
        filter switch
        {
            SchoolKs4CoreSubjectGradeFilter.Grade5 => "5",
            SchoolKs4CoreSubjectGradeFilter.Grade7 => "7",
            _ => "4"
        };

    public static string ToSubjectValue(this SchoolKs4CoreSubject subject) =>
        subject switch
        {
            SchoolKs4CoreSubject.EnglishLiterature => "english-literature",
            SchoolKs4CoreSubject.Biology => "biology",
            SchoolKs4CoreSubject.Chemistry => "chemistry",
            SchoolKs4CoreSubject.Physics => "physics",
            SchoolKs4CoreSubject.Maths => "maths",
            SchoolKs4CoreSubject.CombinedScienceDoubleAward => "combined-science-double-award",
            _ => "english-language"
        };
}
