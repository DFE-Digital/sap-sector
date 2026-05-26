using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

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
        var schoolData = new SchoolData(
            request.Urn,
            schoolDetails.Name,
            await repository.GetByUrnAsync(request.Urn),
            null);

        var similarSchoolUrns = (await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.Urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var similarSchoolData = ((await repository.GetByUrnsAsync(similarSchoolUrns)) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);
        var similarSchoolDetails = ((await establishmentRepository.GetEstablishmentsAsync(similarSchoolUrns))
                ?? Array.Empty<Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchools = similarSchoolUrns
            .Where(similarSchoolDetails.ContainsKey)
            .Select(urn => new SchoolData(
                urn,
                similarSchoolDetails[urn].EstablishmentName,
                similarSchoolData.GetValueOrDefault(urn),
                null))
            .ToArray();

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        var engLangGrade = filterBy.ContainsKey("english-language:grade") ? filterBy["english-language:grade"] : "4";
        var engLang = Measure.Build(
            "english-language",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "english-language:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, engLangGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, engLangGrade == "5"),
                        new FilterOption("7", "Grade 7 and above", 0, engLangGrade == "7")
                    ]),
            ],
            schoolData,
            similarSchools,
            engLangGrade switch
            {
                "5" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
                "7" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct)
            });

        var engLitGrade = filterBy.ContainsKey("english-literature:grade") ? filterBy["english-literature:grade"] : "4";
        var engLit = Measure.Build(
            "english-literature",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "english-literature:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, engLitGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, engLitGrade == "5"),
                        new FilterOption("7", "Grade 7 and above", 0, engLitGrade == "7")
                    ]),
            ],
            schoolData,
            similarSchools,
            engLitGrade switch
            {
                "5" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),
                "7" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct)
            });

        var bioGrade = filterBy.ContainsKey("biology:grade") ? filterBy["biology:grade"] : "4";
        var bio = Measure.Build(
            "biology",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "biology:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, bioGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, bioGrade == "5"),
                        new FilterOption("7", "Grade 7 and above", 0, bioGrade == "7")
                    ]),
            ],
            schoolData,
            similarSchools,
            bioGrade switch
            {
                "5" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),
                "7" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct)
            });

        var chemGrade = filterBy.ContainsKey("chemistry:grade") ? filterBy["chemistry:grade"] : "4";
        var chem = Measure.Build(
            "chemistry",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "chemistry:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, chemGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, chemGrade == "5"),
                        new FilterOption("7", "Grade 7 and above", 0, chemGrade == "7")
                    ]),
            ],
            schoolData,
            similarSchools,
            chemGrade switch
            {
                "5" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),
                "7" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct)
            });

        var physGrade = filterBy.ContainsKey("physics:grade") ? filterBy["physics:grade"] : "4";
        var phys = Measure.Build(
            "physics",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "physics:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, physGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, physGrade == "5"),
                        new FilterOption("7", "Grade 7 and above", 0, physGrade == "7")
                    ]),
            ],
            schoolData,
            similarSchools,
            physGrade switch
            {
                "5" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),
                "7" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct)
            });

        var mathsGrade = filterBy.ContainsKey("maths:grade") ? filterBy["maths:grade"] : "4";
        var maths = Measure.Build(
            "maths",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "maths:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, mathsGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, mathsGrade == "5"),
                        new FilterOption("7", "Grade 7 and above", 0, mathsGrade == "7")
                    ]),
            ],
            schoolData,
            similarSchools,
            mathsGrade switch
            {
                "5" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),
                "7" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct)
            });

        var combSciGrade = filterBy.ContainsKey("combined-science:grade") ? filterBy["combined-science:grade"] : "4";
        var combSci = Measure.Build(
            "combined-science",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "combined-science:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4-4 and above", 0, combSciGrade == "4"),
                        new FilterOption("5", "Grade 5-5 and above", 0, combSciGrade == "5"),
                        new FilterOption("7", "Grade 7-7 and above", 0, combSciGrade == "7")
                    ]),
            ],
            schoolData,
            similarSchools,
            combSciGrade switch
            {
                "5" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct),
                "7" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct)
            });

        return new(
            schoolDetails,
            similarSchools.Length,
            engLang,
            engLit,
            bio,
            chem,
            phys,
            maths,
            combSci);
    }
}

public record GetSchoolKs4CoreSubjectsRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4CoreSubjectsResponse(
    SchoolDetails SchoolDetails,
    int SimilarSchoolsCount,
    Measure EnglishLanguage,
    Measure EnglishLiterature,
    Measure Biology,
    Measure Chemistry,
    Measure Physics,
    Measure Mathematics,
    Measure CombinedScienceDoubleAward);
