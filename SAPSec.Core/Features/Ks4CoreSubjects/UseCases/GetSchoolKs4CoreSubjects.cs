using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public class GetSchoolKs4CoreSubjects(
    IKs4PerformanceRepository repository,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetSchoolKs4CoreSubjectsResponse> Execute(GetSchoolKs4CoreSubjectsRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var schoolData = await repository.GetByUrnAsync(request.Urn);
        var similarSchoolUrns = await similarSchoolsRepository.GetSimilarSchoolUrnsAsync(request.Urn);
        var similarSchoolData = ((await repository.GetByUrnsAsync(similarSchoolUrns)) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);
        var similarSchoolDetails = ((await establishmentRepository.GetEstablishmentsAsync(similarSchoolUrns))
                ?? Array.Empty<SAPSec.Core.Model.Generated.Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchools = similarSchoolUrns
            .Where(similarSchoolDetails.ContainsKey)
            .Select(urn => new SimilarSchoolMeasure(
                urn,
                similarSchoolDetails[urn].EstablishmentName,
                similarSchoolData.GetValueOrDefault(urn)))
            .ToArray();

        return new(
            schoolDetails,
            similarSchools.Length,
            BuildGradeSelections(
                schoolData,
                similarSchools,
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct)),
            BuildGradeSelections(
                schoolData,
                similarSchools,
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct)),
            BuildGradeSelections(
                schoolData,
                similarSchools,
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct),
                new SubjectFieldSelectors(
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct)));
    }

    private static SchoolKs4CoreSubjectsGradeSelections BuildGradeSelections(
        Ks4PerformanceData? schoolData,
        IEnumerable<SimilarSchoolMeasure> similarSchools,
        SubjectFieldSelectors englishLanguageSelectors,
        SubjectFieldSelectors englishLiteratureSelectors,
        SubjectFieldSelectors biologySelectors,
        SubjectFieldSelectors chemistrySelectors,
        SubjectFieldSelectors physicsSelectors,
        SubjectFieldSelectors mathsSelectors,
        SubjectFieldSelectors combinedScienceSelectors) =>
        new(
            BuildSelection(schoolData, similarSchools, englishLanguageSelectors),
            BuildSelection(schoolData, similarSchools, englishLiteratureSelectors),
            BuildSelection(schoolData, similarSchools, biologySelectors),
            BuildSelection(schoolData, similarSchools, chemistrySelectors),
            BuildSelection(schoolData, similarSchools, physicsSelectors),
            BuildSelection(schoolData, similarSchools, mathsSelectors),
            BuildSelection(schoolData, similarSchools, combinedScienceSelectors));

    private static SchoolKs4CoreSubjectSelection BuildSelection(
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

public record GetSchoolKs4CoreSubjectsRequest(string Urn);

public enum SchoolKs4CoreSubject
{
    EnglishLanguage,
    EnglishLiterature,
    Biology,
    Chemistry,
    Physics,
    Maths,
    CombinedScienceDoubleAward
}

public enum SchoolKs4CoreSubjectGradeFilter
{
    Grade4,
    Grade5,
    Grade7
}

public record SchoolKs4CoreSubjectSelection(
    SchoolKs4ComparisonAverage ThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> TopPerformers,
    SchoolKs4ComparisonYearByYear YearByYear)
{
    public static SchoolKs4CoreSubjectSelection From(
        GetSchoolKs4CoreSubjectsResponse response,
        SchoolKs4CoreSubject subject,
        SchoolKs4CoreSubjectGradeFilter grade) =>
        (subject, grade) switch
        {
            (SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade5) => response.Grade5AndAbove.CombinedScienceDoubleAward,
            (SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade7) => response.Grade7AndAbove.CombinedScienceDoubleAward,
            (SchoolKs4CoreSubject.CombinedScienceDoubleAward, _) => response.Grade4AndAbove.CombinedScienceDoubleAward,
            (SchoolKs4CoreSubject.Biology, SchoolKs4CoreSubjectGradeFilter.Grade5) => response.Grade5AndAbove.Biology,
            (SchoolKs4CoreSubject.Biology, SchoolKs4CoreSubjectGradeFilter.Grade7) => response.Grade7AndAbove.Biology,
            (SchoolKs4CoreSubject.Biology, _) => response.Grade4AndAbove.Biology,
            (SchoolKs4CoreSubject.Chemistry, SchoolKs4CoreSubjectGradeFilter.Grade5) => response.Grade5AndAbove.Chemistry,
            (SchoolKs4CoreSubject.Chemistry, SchoolKs4CoreSubjectGradeFilter.Grade7) => response.Grade7AndAbove.Chemistry,
            (SchoolKs4CoreSubject.Chemistry, _) => response.Grade4AndAbove.Chemistry,
            (SchoolKs4CoreSubject.Physics, SchoolKs4CoreSubjectGradeFilter.Grade5) => response.Grade5AndAbove.Physics,
            (SchoolKs4CoreSubject.Physics, SchoolKs4CoreSubjectGradeFilter.Grade7) => response.Grade7AndAbove.Physics,
            (SchoolKs4CoreSubject.Physics, _) => response.Grade4AndAbove.Physics,
            (SchoolKs4CoreSubject.Maths, SchoolKs4CoreSubjectGradeFilter.Grade5) => response.Grade5AndAbove.Maths,
            (SchoolKs4CoreSubject.Maths, SchoolKs4CoreSubjectGradeFilter.Grade7) => response.Grade7AndAbove.Maths,
            (SchoolKs4CoreSubject.Maths, _) => response.Grade4AndAbove.Maths,
            (SchoolKs4CoreSubject.EnglishLiterature, SchoolKs4CoreSubjectGradeFilter.Grade5) => response.Grade5AndAbove.EnglishLiterature,
            (SchoolKs4CoreSubject.EnglishLiterature, SchoolKs4CoreSubjectGradeFilter.Grade7) => response.Grade7AndAbove.EnglishLiterature,
            (SchoolKs4CoreSubject.EnglishLiterature, _) => response.Grade4AndAbove.EnglishLiterature,
            (SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade5) => response.Grade5AndAbove.EnglishLanguage,
            (SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade7) => response.Grade7AndAbove.EnglishLanguage,
            _ => response.Grade4AndAbove.EnglishLanguage
        };
}

public record SchoolKs4CoreSubjectsGradeSelections(
    SchoolKs4CoreSubjectSelection EnglishLanguage,
    SchoolKs4CoreSubjectSelection EnglishLiterature,
    SchoolKs4CoreSubjectSelection Biology,
    SchoolKs4CoreSubjectSelection Chemistry,
    SchoolKs4CoreSubjectSelection Physics,
    SchoolKs4CoreSubjectSelection Maths,
    SchoolKs4CoreSubjectSelection CombinedScienceDoubleAward);

public record GetSchoolKs4CoreSubjectsResponse(
    SchoolDetails SchoolDetails,
    int SimilarSchoolsCount,
    SchoolKs4CoreSubjectsGradeSelections Grade4AndAbove,
    SchoolKs4CoreSubjectsGradeSelections Grade5AndAbove,
    SchoolKs4CoreSubjectsGradeSelections Grade7AndAbove);

internal sealed record SubjectFieldSelectors(
    Func<Ks4PerformanceData?, string?> SchoolCurrent,
    Func<Ks4PerformanceData?, string?> SchoolPrevious,
    Func<Ks4PerformanceData?, string?> SchoolPrevious2,
    Func<Ks4PerformanceData?, string?> LocalAuthorityCurrent,
    Func<Ks4PerformanceData?, string?> LocalAuthorityPrevious,
    Func<Ks4PerformanceData?, string?> LocalAuthorityPrevious2,
    Func<Ks4PerformanceData?, string?> EnglandCurrent,
    Func<Ks4PerformanceData?, string?> EnglandPrevious,
    Func<Ks4PerformanceData?, string?> EnglandPrevious2);

internal sealed record SimilarSchoolMeasure(string Urn, string Name, Ks4PerformanceData? Data);
