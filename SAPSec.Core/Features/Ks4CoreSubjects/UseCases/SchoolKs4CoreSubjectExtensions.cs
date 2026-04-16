namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public static class SchoolKs4CoreSubjectExtensions
{
    public static SchoolKs4CoreSubjectGradeFilter ParseFilter(string? grade) =>
        grade switch
        {
            "5" => SchoolKs4CoreSubjectGradeFilter.Grade5,
            "7" => SchoolKs4CoreSubjectGradeFilter.Grade7,
            "4" => SchoolKs4CoreSubjectGradeFilter.Grade4,
            _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, "Unsupported KS4 core subject grade filter.")
        };

    public static SchoolKs4CoreSubject ParseSubject(string? subject) =>
        subject?.ToLowerInvariant() switch
        {
            "english-language" => SchoolKs4CoreSubject.EnglishLanguage,
            "english-literature" => SchoolKs4CoreSubject.EnglishLiterature,
            "biology" => SchoolKs4CoreSubject.Biology,
            "chemistry" => SchoolKs4CoreSubject.Chemistry,
            "physics" => SchoolKs4CoreSubject.Physics,
            "maths" => SchoolKs4CoreSubject.Maths,
            "combined-science-double-award" => SchoolKs4CoreSubject.CombinedScienceDoubleAward,
            _ => throw new ArgumentOutOfRangeException(nameof(subject), subject, "Unsupported KS4 core subject.")
        };

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
            SchoolKs4CoreSubject.EnglishLanguage => "english-language",
            SchoolKs4CoreSubject.EnglishLiterature => "english-literature",
            SchoolKs4CoreSubject.Biology => "biology",
            SchoolKs4CoreSubject.Chemistry => "chemistry",
            SchoolKs4CoreSubject.Physics => "physics",
            SchoolKs4CoreSubject.Maths => "maths",
            SchoolKs4CoreSubject.CombinedScienceDoubleAward => "combined-science-double-award",
            _ => throw new ArgumentOutOfRangeException(nameof(subject), subject, "Unsupported KS4 core subject.")
        };
}
