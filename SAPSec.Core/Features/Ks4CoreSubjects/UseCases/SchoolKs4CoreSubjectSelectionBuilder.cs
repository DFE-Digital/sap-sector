using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

internal static class SchoolKs4CoreSubjectSelectionBuilder
{
    public static SchoolKs4CoreSubjectSelection BuildSelection(
        Ks4PerformanceData? schoolData,
        IEnumerable<SimilarSchoolMeasure> similarSchools,
        SchoolKs4CoreSubject subject,
        SchoolKs4CoreSubjectGradeFilter grade) =>
        BuildSelection(schoolData, similarSchools, GetSelectors(subject, grade));

    public static SchoolKs4CoreSubjectSelection BuildSelection(
        Ks4PerformanceData? schoolData,
        IEnumerable<SimilarSchoolMeasure> similarSchools,
        SubjectFieldSelectors selectors)
    {
        var schoolAverage = BuildAverage(
            selectors.SchoolCurrent(schoolData),
            selectors.SchoolPrevious(schoolData),
            selectors.SchoolPrevious2(schoolData),
            selectors.LocalAuthorityCurrent(schoolData),
            selectors.LocalAuthorityPrevious(schoolData),
            selectors.LocalAuthorityPrevious2(schoolData),
            selectors.EnglandCurrent(schoolData),
            selectors.EnglandPrevious(schoolData),
            selectors.EnglandPrevious2(schoolData));
        var schoolYearByYear = BuildYearByYear(
            selectors.SchoolCurrent(schoolData),
            selectors.SchoolPrevious(schoolData),
            selectors.SchoolPrevious2(schoolData),
            selectors.LocalAuthorityCurrent(schoolData),
            selectors.LocalAuthorityPrevious(schoolData),
            selectors.LocalAuthorityPrevious2(schoolData),
            selectors.EnglandCurrent(schoolData),
            selectors.EnglandPrevious(schoolData),
            selectors.EnglandPrevious2(schoolData));

        return new(
            new SchoolKs4ComparisonAverage(
                schoolAverage.SchoolValue,
                Average(similarSchools.Select(x => MeasureValue(
                    selectors.SchoolCurrent(x.Data),
                    selectors.SchoolPrevious(x.Data),
                    selectors.SchoolPrevious2(x.Data)))),
                schoolAverage.LocalAuthorityValue,
                schoolAverage.EnglandValue),
            BuildTopPerformers(
                similarSchools,
                x => MeasureValue(
                    selectors.SchoolCurrent(x.Data),
                    selectors.SchoolPrevious(x.Data),
                    selectors.SchoolPrevious2(x.Data))),
            BuildComparisonYearByYear(
                schoolYearByYear,
                similarSchools.Select(x => SeriesFrom(
                    selectors.SchoolCurrent(x.Data),
                    selectors.SchoolPrevious(x.Data),
                    selectors.SchoolPrevious2(x.Data)))));
    }

    private static SubjectFieldSelectors GetSelectors(
        SchoolKs4CoreSubject subject,
        SchoolKs4CoreSubjectGradeFilter grade) =>
        (subject, grade) switch
        {
            (SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade4) => new(
                x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.EnglishLiterature, SchoolKs4CoreSubjectGradeFilter.Grade4) => new(
                x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Biology, SchoolKs4CoreSubjectGradeFilter.Grade4) => new(
                x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Chemistry, SchoolKs4CoreSubjectGradeFilter.Grade4) => new(
                x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Physics, SchoolKs4CoreSubjectGradeFilter.Grade4) => new(
                x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Maths, SchoolKs4CoreSubjectGradeFilter.Grade4) => new(
                x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade4) => new(
                x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade5) => new(
                x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.EnglishLiterature, SchoolKs4CoreSubjectGradeFilter.Grade5) => new(
                x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Biology, SchoolKs4CoreSubjectGradeFilter.Grade5) => new(
                x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Chemistry, SchoolKs4CoreSubjectGradeFilter.Grade5) => new(
                x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Physics, SchoolKs4CoreSubjectGradeFilter.Grade5) => new(
                x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Maths, SchoolKs4CoreSubjectGradeFilter.Grade5) => new(
                x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade5) => new(
                x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade7) => new(
                x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.EnglishLiterature, SchoolKs4CoreSubjectGradeFilter.Grade7) => new(
                x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Biology, SchoolKs4CoreSubjectGradeFilter.Grade7) => new(
                x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Chemistry, SchoolKs4CoreSubjectGradeFilter.Grade7) => new(
                x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Physics, SchoolKs4CoreSubjectGradeFilter.Grade7) => new(
                x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.Maths, SchoolKs4CoreSubjectGradeFilter.Grade7) => new(
                x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct),
            (SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade7) => new(
                x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct),
            _ => throw new ArgumentOutOfRangeException(nameof(subject), subject, "Unsupported KS4 core subject selection.")
        };

    private static Ks4HeadlineMeasureAverage BuildAverage(
        string? schoolCurrent,
        string? schoolPrevious,
        string? schoolPrevious2,
        string? localAuthorityCurrent,
        string? localAuthorityPrevious,
        string? localAuthorityPrevious2,
        string? englandCurrent,
        string? englandPrevious,
        string? englandPrevious2) =>
        Ks4HeadlineMeasuresCalculator.BuildAverage(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolPrevious2),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityPrevious2),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandPrevious2));

    private static Ks4HeadlineMeasureYearByYear BuildYearByYear(
        string? schoolCurrent,
        string? schoolPrevious,
        string? schoolPrevious2,
        string? localAuthorityCurrent,
        string? localAuthorityPrevious,
        string? localAuthorityPrevious2,
        string? englandCurrent,
        string? englandPrevious,
        string? englandPrevious2) =>
        Ks4HeadlineMeasuresCalculator.BuildYearByYear(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolPrevious2),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityPrevious2),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandPrevious2));

    private static SchoolKs4ComparisonYearByYear BuildComparisonYearByYear(
        Ks4HeadlineMeasureYearByYear current,
        IEnumerable<Ks4HeadlineMeasureSeries> similarSchoolSeries)
    {
        var similarSeries = similarSchoolSeries.ToArray();

        return new(
            current.School,
            new Ks4HeadlineMeasureSeries(
                Average(similarSeries.Select(x => x.Current)),
                Average(similarSeries.Select(x => x.Previous)),
                Average(similarSeries.Select(x => x.Previous2))),
            current.LocalAuthority,
            current.England);
    }

    private static decimal? MeasureValue(string? current, string? previous, string? previous2) =>
        Ks4HeadlineMeasuresCalculator.Average(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(current),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous2));

    private static Ks4HeadlineMeasureSeries SeriesFrom(string? current, string? previous, string? previous2) =>
        new(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(current),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous2));

    private static decimal? Average(IEnumerable<decimal?> values)
    {
        var availableValues = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return availableValues.Count == 0
            ? null
            : Math.Round(availableValues.Average(), 1, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyList<Ks4TopPerformer> BuildTopPerformers(
        IEnumerable<SimilarSchoolMeasure> similarSchoolResponses,
        Func<SimilarSchoolMeasure, decimal?> selector) =>
        similarSchoolResponses
            .Select(response => new
            {
                response.Urn,
                response.Name,
                Value = selector(response)
            })
            .Where(x => x.Value.HasValue)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((x, index) => new Ks4TopPerformer(index + 1, x.Urn, x.Name, x.Value))
            .ToList()
            .AsReadOnly();
}
