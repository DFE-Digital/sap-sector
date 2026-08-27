using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public class ComparisonMeasureDataProvider<T>(
    IEstablishmentRepository establishmentRepository,
    IMeasureDataRepository<T> repository) : IComparisonMeasureDataProvider<T>
    where T : class, IMeasureData
{
    public async Task<ComparisonMeasureData<T>> GetData(
        string currentSchoolUrn,
        string similarSchoolUrn)
    {
        var urns = new[] { currentSchoolUrn, similarSchoolUrn };

        var schools = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(currentSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        if (!schools.ContainsKey(similarSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {similarSchoolUrn}");
        }

        var absence = (await repository.GetByUrnsAsync(urns))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolMeasureData<T>(
            schools[currentSchoolUrn],
            absence.TryGetValue(currentSchoolUrn, out var currentAbsence) ? currentAbsence : null);

        var similarSchoolData = new SchoolMeasureData<T>(
            schools[similarSchoolUrn],
            absence.TryGetValue(similarSchoolUrn, out var similarAbsence) ? similarAbsence : null);

        return new(currentSchoolData, similarSchoolData);
    }
}
