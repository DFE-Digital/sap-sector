using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetSchoolKs4HeadlineMeasures(
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetSchoolKs4HeadlineMeasuresResponse> Execute(GetSchoolKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var performance = await performanceRepository.GetByUrnAsync(request.Urn);
        var dest = await destinationsRepository.GetByUrnAsync(request.Urn);

        var schoolData = new SchoolData(
            request.Urn,
            schoolDetails.Name,
            await performanceRepository.GetByUrnAsync(request.Urn),
            await destinationsRepository.GetByUrnAsync(request.Urn));

        var similarSchoolUrns = (await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.Urn))
            .Select(s => s.NeighbourURN);
        var similarSchoolPerformanceData = ((await performanceRepository.GetByUrnsAsync(similarSchoolUrns)) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);
        var similarSchoolDestinationsData = ((await destinationsRepository.GetByUrnsAsync(similarSchoolUrns)) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);
        var similarSchoolDetails = ((await establishmentRepository.GetEstablishmentsAsync(similarSchoolUrns))
                ?? Array.Empty<Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchools = similarSchoolUrns
            .Where(similarSchoolDetails.ContainsKey)
            .Select(urn => new SchoolData(
                urn,
                similarSchoolDetails[urn].EstablishmentName,
                similarSchoolPerformanceData.GetValueOrDefault(urn),
                similarSchoolDestinationsData.GetValueOrDefault(urn)))
            .ToArray();

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        var attainment8 = SchoolMeasure.Build(
            "attainment8",
            "Attainment 8",
            MeasureDataType.Number,
            [],
            schoolData,
            similarSchools,
            new(
                x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                x => x?.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num,
                x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num,
                x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num,
                x => x?.PerformanceData?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num,
                x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num,
                x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num,
                x => x?.PerformanceData?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num));

        var engMathsGrade = filterBy.ContainsKey("eng-maths:grade") ? filterBy["eng-maths:grade"] : "4";

        var engMaths = SchoolMeasure.Build(
            "eng-maths",
            "Grade achieved in English and maths GCSEs",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "eng-maths:grade",
                    "Grade", [
                        new FilterOption("4", "Grade 4 and above", 0, engMathsGrade == "4"),
                        new FilterOption("5", "Grade 5 and above", 0, engMathsGrade == "5")
                    ]),
            ],
            schoolData,
            similarSchools,
            engMathsGrade switch
            {
                "5" => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)
            });

        var destinationType = filterBy.ContainsKey("destinations:destination") ? filterBy["destinations:destination"] : "all";
        var destinations = SchoolMeasure.Build(
            "destinations",
            "Staying in education or entering employment",
            MeasureDataType.Percentage,
            [
                new MeasureAvailableFilter(
                    "destinations:destination",
                    "Destination", [
                        new FilterOption("all", "All destinations", 0, destinationType == "all"),
                        new FilterOption("education", "Education", 0, engMathsGrade == "education"),
                        new FilterOption("employment", "Employment and apprenticeships", 0, engMathsGrade == "employment")
                    ])
            ],
            schoolData,
            similarSchools,
            destinationType switch
            {
                "education" => new(
                    x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                    x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                    x => x?.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Current_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct),
                "employment" => new(
                    x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                    x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                    x => x?.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct),
                _ => new(
                    x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                    x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                    x => x?.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct,
                    x => x?.DestinationsData?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct,
                    x => x?.DestinationsData?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)
            });

        return new(
            schoolDetails,
            similarSchools.Length,
            attainment8,
            engMaths,
            destinations);
    }
}

public record GetSchoolKs4HeadlineMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4HeadlineMeasuresResponse(
    SchoolDetails SchoolDetails,
    int SimilarSchoolsCount,
    SchoolMeasure Attainment8,
    SchoolMeasure EnglishAndMaths,
    SchoolMeasure Destinations);