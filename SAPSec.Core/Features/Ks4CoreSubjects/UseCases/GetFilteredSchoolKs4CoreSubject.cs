using SAPSec.Core.Interfaces.Services;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public class GetFilteredSchoolKs4CoreSubject(
    IKs4PerformanceStore store,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore)
{
    public async Task<GetFilteredSchoolKs4CoreSubjectResponse> Execute(GetFilteredSchoolKs4CoreSubjectRequest request)
    {
        var gradeFilter = SchoolKs4CoreSubjectExtensions.ParseFilter(request.Grade);
        var subjectFilter = SchoolKs4CoreSubjectExtensions.ParseSubject(request.Subject);
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var schoolData = await store.GetByUrnAsync(request.Urn);
        var similarSchoolUrns = (await similarSchoolsStore.GetGroupAsync(request.Urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var similarSchoolData = (await store.GetByUrnsAsync(similarSchoolUrns) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);
        var similarSchoolDetails = (await establishmentStore.GetEstablishmentsAsync(similarSchoolUrns)
                ?? Array.Empty<Establishment>())
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
            SchoolKs4CoreSubjectSelectionBuilder.BuildSelection(schoolData, schoolDetails, similarSchools, subjectFilter, gradeFilter),
            subjectFilter,
            gradeFilter);
    }
}

public record GetFilteredSchoolKs4CoreSubjectRequest(
    string Urn,
    string? Subject,
    string? Grade);

public record GetFilteredSchoolKs4CoreSubjectResponse(
    SchoolKs4CoreSubjectSelection Selection,
    SchoolKs4CoreSubject Subject,
    SchoolKs4CoreSubjectGradeFilter Grade);
