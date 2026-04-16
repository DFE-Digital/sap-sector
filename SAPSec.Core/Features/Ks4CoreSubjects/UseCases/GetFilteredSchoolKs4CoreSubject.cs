using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;

namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public class GetFilteredSchoolKs4CoreSubject(
    IKs4PerformanceRepository repository,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetFilteredSchoolKs4CoreSubjectResponse> Execute(GetFilteredSchoolKs4CoreSubjectRequest request)
    {
        var gradeFilter = SchoolKs4CoreSubjectExtensions.ParseFilter(request.Grade);
        var subjectFilter = SchoolKs4CoreSubjectExtensions.ParseSubject(request.Subject);
        var schoolData = await repository.GetByUrnAsync(request.Urn);
        var similarSchoolUrns = (await similarSchoolsRepository.GetSimilarSchoolUrnsAsync(request.Urn))
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
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
            SchoolKs4CoreSubjectSelectionBuilder.BuildSelection(schoolData, similarSchools, subjectFilter, gradeFilter),
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
