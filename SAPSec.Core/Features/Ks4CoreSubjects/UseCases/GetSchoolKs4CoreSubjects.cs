using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public class GetSchoolKs4CoreSubjects(
    IKs4PerformanceRepository performanceRepository,
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
            await performanceRepository.GetByUrnAsync(request.Urn),
            null);

        var similarSchoolUrns = (await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.Urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var similarSchoolData = ((await performanceRepository.GetByUrnsAsync(similarSchoolUrns)) ?? [])
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

        return new(
            schoolDetails,
            similarSchools.Length,
            [
                Measures.Ks4CoreSubjects.EnglishLanguage.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.EnglishLiterature.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.Biology.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.Chemistry.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.Physics.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.Mathematics.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.CombinedScience.ForSchool(schoolData, similarSchools, filterBy),
            ]);
    }
}

public record GetSchoolKs4CoreSubjectsRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4CoreSubjectsResponse(
    SchoolDetails SchoolDetails,
    int SimilarSchoolsCount,
    IReadOnlyCollection<Measure> Measures);
