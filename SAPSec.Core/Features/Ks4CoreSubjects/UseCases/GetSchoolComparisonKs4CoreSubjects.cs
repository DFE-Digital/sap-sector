using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public class GetSchoolComparisonKs4CoreSubjects(
    IKs4PerformanceRepository performanceRepository,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetSchoolComparisonKs4CoreSubjectsResponse> Execute(GetSchoolComparisonKs4CoreSubjectsRequest request)
    {
        var currentSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.CurrentSchoolUrn);
        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.SimilarSchoolUrn);

        var currentSchoolData = new SchoolData(
            request.CurrentSchoolUrn,
            currentSchoolDetails.Name,
            await performanceRepository.GetByUrnAsync(request.CurrentSchoolUrn),
            null);

        var similarSchoolsUrns = (await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var similarSchoolsPerformance = ((await performanceRepository.GetByUrnsAsync(similarSchoolsUrns)) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);

        var similarSchoolsDetails = ((await establishmentRepository.GetEstablishmentsAsync(similarSchoolsUrns))
                ?? Array.Empty<Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchoolsData = similarSchoolsUrns
            .Where(similarSchoolsDetails.ContainsKey)
            .Select(urn => new SchoolData(
                urn,
                similarSchoolsDetails[urn].EstablishmentName,
                similarSchoolsPerformance.GetValueOrDefault(urn),
                null))
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);

        var similarSchoolData = similarSchoolsData[request.SimilarSchoolUrn];

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        IReadOnlyCollection<SchoolComparisonMeasure> measures = [
            BuildSubjectMeasure(
                "english-language",
                "English language",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct)),
            BuildSubjectMeasure(
                "english-literature",
                "English literature",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct)),
            BuildSubjectMeasure(
                "biology",
                "Biology",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct)),
            BuildSubjectMeasure(
                "chemistry",
                "Chemistry",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct)),
            BuildSubjectMeasure(
                "physics",
                "Physics",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct)),
            BuildSubjectMeasure(
                "maths",
                "Mathematics",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct)),
            BuildSubjectMeasure(
                "combined-science",
                "Combined science (double award)",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct),
                hasCombinedGrades: true)
        ];

        return new(
            currentSchoolDetails,
            similarSchoolDetails,
            similarSchoolsData.Values.Count,
            measures);

        SchoolComparisonMeasure BuildSubjectMeasure(
            string key,
            string name,
            MeasureFieldSelector grade4Fields,
            MeasureFieldSelector grade5Fields,
            MeasureFieldSelector grade7Fields,
            bool hasCombinedGrades = false)
        {
            var filterKey = $"{key}:grade";
            var grade = filterBy.ContainsKey(filterKey) ? filterBy[filterKey] : "4";

            return SchoolComparisonMeasure.Build(
                key,
                name,
                MeasureDataType.Percentage,
                [
                    new MeasureAvailableFilter(
                        filterKey,
                        "Grade", [
                            new FilterOption("4", hasCombinedGrades ? "Grade 4-4 and above" : "Grade 4 and above", 0, grade == "4"),
                            new FilterOption("5", hasCombinedGrades ? "Grade 5-5 and above" : "Grade 5 and above", 0, grade == "5"),
                            new FilterOption("7", hasCombinedGrades ? "Grade 7-7 and above" : "Grade 7 and above", 0, grade == "7")
                        ]),
                ],
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData.Values,
                grade switch
                {
                    "5" => grade5Fields,
                    "7" => grade7Fields,
                    _ => grade4Fields
                });
        }
    }
}

public record GetSchoolComparisonKs4CoreSubjectsRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolComparisonKs4CoreSubjectsResponse(
    SchoolDetails CurrentSchoolDetails,
    SchoolDetails SimilarSchoolDetails,
    int SimilarSchoolsCount,
    IReadOnlyCollection<SchoolComparisonMeasure> Measures);
